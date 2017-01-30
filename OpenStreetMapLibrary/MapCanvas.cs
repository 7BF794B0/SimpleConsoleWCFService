using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenStreetMapLibrary
{
    /// <summary>
    /// Displays a map using data from openstreetmap.org.
    /// </summary>
    public class MapCanvas : Canvas
    {
        /// <summary>
        /// Private helper class to handle the X/Y offsets of the TilePanel.
        /// </summary>
        private class MapOffset
        {
            private readonly EventHandler _offsetChanged;
            private readonly TileGenerator _tileGenerator;

            private double _mapSize = TileGenerator.TileSize; // Default to zoom 0
            private double _offset;
            private double _size;
            private int _maximumTile;
            private int _tile;

            // Used for animation
            private bool _animating;
            private double _step;
            private double _target;
            private double _value;

            /// <summary>
            /// Initializes a new instance of the MapOffset class.
            /// </summary>
            /// <param name="property">The property this MapOffset represents.</param>
            /// <param name="offsetChanged">Called when the Offset changes.</param>
            /// <param name="tileGenerator">TileGenerator</param>
            internal MapOffset(PropertyInfo property, EventHandler offsetChanged, TileGenerator tileGenerator)
            {
                System.Diagnostics.Debug.Assert(property != null, "property cannot be null");
                System.Diagnostics.Debug.Assert(offsetChanged != null, "offsetChanged cannot be null");

                _offsetChanged = offsetChanged;
                Property = property;
                _tileGenerator = tileGenerator;
                CompositionTarget.Rendering += OnRendering; // Used for manual animation
            }

            /// <summary>
            /// Gets the offset from the tile edge to the screen edge.
            /// </summary>
            public double Offset
            {
                get
                {
                    return _offset;
                }
                private set
                {
                    if (_offset != value)
                    {
                        _offset = value;
                        _offsetChanged(this, EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Gets the location of the starting tile in pixels.
            /// </summary>
            public double Pixels => (Tile * TileGenerator.TileSize) - Offset;

            /// <summary>
            /// Gets the PropertyInfo associated with this offset.
            /// </summary>
            /// <remarks>This is used so a generic handler can be called for the _offsetChanged delegate.</remarks>
            public PropertyInfo Property { get; }

            /// <summary>
            /// Gets the starting tile index.
            /// </summary>
            public int Tile
            {
                get
                {
                    return _tile;
                }
                private set
                {
                    if (_tile != value)
                    {
                        _tile = value;
                    }
                }
            }

            /// <summary>
            /// Adjusts the offset so the specifed tile is in the center of the control.
            /// </summary>
            /// <param name="tile">The tile (allowing fractions of the tile) to be centered.</param>
            public void CenterOn(double tile)
            {
                double pixels = (tile * TileGenerator.TileSize) - (_size / 2.0);
                Translate(Pixels - pixels);
            }

            /// <summary>
            /// Called when the size of the parent control changes.
            /// </summary>
            /// <param name="size">The nes size of the parent control.</param>
            public void ChangeSize(double size)
            {
                _size = size;
                // Only interested in the integer part, the rest will be truncated.
                _maximumTile = (int)((_mapSize - _size) / TileGenerator.TileSize);
                // Force a refresh.
                Translate(0);
            }

            /// <summary>
            /// Updates the starting tile index based on the zoom level.
            /// </summary>
            /// <param name="zoom">The zoom level.</param>
            /// <param name="offset">The distance from the edge to keep the same when changing zoom.</param>
            public void ChangeZoom(int zoom, double offset)
            {
                int currentZoom = _tileGenerator.GetZoom(_mapSize / TileGenerator.TileSize);
                if (currentZoom != zoom)
                {
                    _animating = false;

                    double scale = Math.Pow(2, zoom - currentZoom); // 2^delta
                    double location = ((Pixels + offset) * scale) - offset; // Bias new location on the offset

                    _mapSize = _tileGenerator.GetSize(zoom);
                    _maximumTile = (int)((_mapSize - _size) / TileGenerator.TileSize);

                    Translate(Pixels - location);
                }
            }

            /// <summary>
            /// Changes the offset by the specified amount.
            /// </summary>
            /// <param name="value">The amount to change the offset by.</param>
            public void Translate(double value)
            {
                if (_size > _mapSize)
                {
                    Tile = 0;
                    Offset = (_size - _mapSize) / 2;
                }
                else
                {
                    double location = Pixels - value;
                    if (location < 0)
                    {
                        Tile = 0;
                        Offset = 0;
                    }
                    else if (location + _size > _mapSize)
                    {
                        Tile = _maximumTile;
                        Offset = _size - (_mapSize - (_maximumTile * TileGenerator.TileSize));
                    }
                    else
                    {
                        Tile = (int)(location / TileGenerator.TileSize);
                        Offset = (Tile * TileGenerator.TileSize) - location;
                    }
                }
            }

            // Used for animating the Translate.
            private void OnRendering(object sender, EventArgs e)
            {
                if (_animating)
                {
                    Translate(_step);
                    _value += Math.Abs(_step);
                    // Stop animating once we've reached/exceeded the target.
                    _animating = _value < _target;
                }
            }
        }

        /// <summary>
        /// Identifies the Latitude attached property.
        /// </summary>
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.RegisterAttached("Latitude", typeof(double), typeof(MapCanvas), new PropertyMetadata(double.PositiveInfinity, OnLatitudeLongitudePropertyChanged));

        /// <summary>
        /// Identifies the Longitude attached property.
        /// </summary>
        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.RegisterAttached("Longitude", typeof(double), typeof(MapCanvas), new PropertyMetadata(double.PositiveInfinity, OnLatitudeLongitudePropertyChanged));

        /// <summary>
        /// Identifies the Viewport dependency property.
        /// </summary>
        public readonly DependencyProperty ViewportProperty;

        /// <summary>
        /// Identifies the Zoom dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(int), typeof(MapCanvas), new UIPropertyMetadata(0, OnZoomPropertyChanged, OnZoomPropertyCoerceValue));

        private static readonly DependencyPropertyKey ViewportKey = DependencyProperty.RegisterReadOnly("Viewport", typeof(Rect), typeof(MapCanvas), new PropertyMetadata());

        private readonly TileGenerator _tileGenerator = new TileGenerator();
        private readonly TilePanel _tilePanel;
        private readonly Image _cache = new Image();
        private int _updateCount;
        private bool _mouseCaptured;
        private Point _previousMouse;
        private readonly MapOffset _offsetX;
        private readonly MapOffset _offsetY;
        private readonly TranslateTransform _translate = new TranslateTransform();

        /// <summary>
        /// Initializes a new instance of the MapCanvas class.
        /// </summary>
        public MapCanvas()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas),
                new CommandBinding(NavigationCommands.DecreaseZoom, (sender, e) => ((MapCanvas)sender).Zoom--));

            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas),
                new CommandBinding(NavigationCommands.IncreaseZoom, (sender, e) => ((MapCanvas)sender).Zoom++));

            _offsetX = new MapOffset(_translate.GetType().GetProperty("X"), OnOffsetChanged, _tileGenerator);
            _offsetY = new MapOffset(_translate.GetType().GetProperty("Y"), OnOffsetChanged, _tileGenerator);

            _tilePanel = new TilePanel(_tileGenerator) { RenderTransform = _translate };
            // Register all mouse clicks.
            Background = Brushes.Transparent;
            Children.Add(_cache);
            Children.Add(_tilePanel);
            ClipToBounds = true;
            Focusable = true;
            FocusVisualStyle = null;
            SnapsToDevicePixels = true;

            // Need to set it here after ViewportKey has been initialized.
            ViewportProperty = ViewportKey.DependencyProperty;
        }

        /// <summary>
        /// Gets the visible area of the map in latitude/longitude coordinates.
        /// </summary>
        public Rect Viewport
        {
            get { return (Rect)GetValue(ViewportProperty); }
            private set { SetValue(ViewportKey, value); }
        }

        /// <summary>
        /// Gets or sets the zoom level of the map.
        /// </summary>
        public int Zoom
        {
            get { return (int)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        /// <summary>
        /// Gets the value of the Latitude attached property for a given depencency object.
        /// </summary>
        /// <param name="obj">The element from which the property value is read.</param>
        /// <returns>The Latitude coordinate of the specified element.</returns>
        public double GetLatitude(DependencyObject obj)
        {
            return (double)obj.GetValue(LatitudeProperty);
        }

        /// <summary>
        /// Gets the value of the Longitude attached property for a given depencency object.
        /// </summary>
        /// <param name="obj">The element from which the property value is read.</param>
        /// <returns>The Longitude coordinate of the specified element.</returns>
        public double GetLongitude(DependencyObject obj)
        {
            return (double)obj.GetValue(LongitudeProperty);
        }

        /// <summary>
        /// Sets the value of the Latitude attached property for a given depencency object.
        /// </summary>
        /// <param name="obj">The element to which the property value is written.</param>
        /// <param name="value">Sets the Latitude coordinate of the specified element.</param>
        public static void SetLatitude(DependencyObject obj, double value)
        {
            obj.SetValue(LatitudeProperty, value);
        }

        /// <summary>
        /// Sets the value of the Longitude attached property for a given depencency object.
        /// </summary>
        /// <param name="obj">The element to which the property value is written.</param>
        /// <param name="value">Sets the Longitude coordinate of the specified element.</param>
        public static void SetLongitude(DependencyObject obj, double value)
        {
            obj.SetValue(LongitudeProperty, value);
        }

        /// <summary>
        /// Centers the map on the specified coordinates.
        /// </summary>
        /// <param name="latitude">The latitude cooridinate.</param>
        /// <param name="longitude">The longitude coordinates.</param>
        /// <param name="zoom">The zoom level for the map.</param>
        public void Center(double latitude, double longitude, int zoom)
        {
            BeginUpdate();
            Zoom = zoom;
            _offsetX.CenterOn(_tileGenerator.GetTileX(longitude, Zoom));
            _offsetY.CenterOn(_tileGenerator.GetTileY(latitude, Zoom));
            EndUpdate();
        }

        /// <summary>
        /// Centers the map on the specified coordinates, calculating the required zoom level.
        /// </summary>
        /// <param name="latitude">The latitude cooridinate.</param>
        /// <param name="longitude">The longitude coordinates.</param>
        /// <param name="size">The minimum size that must be visible, centered on the coordinates.</param>
        public void Center(double latitude, double longitude, Size size)
        {
            double left = _tileGenerator.GetTileX(longitude - size.Width / 2.0, 0);
            double right = _tileGenerator.GetTileX(longitude + size.Width / 2.0, 0);
            double top = _tileGenerator.GetTileY(latitude - size.Height / 2.0, 0);
            double bottom = _tileGenerator.GetTileY(latitude + size.Height / 2.0, 0);

            double height = (top - bottom) * TileGenerator.TileSize;
            double width = (right - left) * TileGenerator.TileSize;
            int zoom = Math.Min(_tileGenerator.GetZoom(ActualHeight / height), _tileGenerator.GetZoom(ActualWidth / width));
            Center(latitude, longitude, zoom);
        }

        /// <summary>
        /// Creates a static image of the current view.
        /// </summary>
        /// <returns>An image of the current map.</returns>
        public ImageSource CreateImage()
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Default);
            bitmap.Render(_tilePanel);
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// Calculates the coordinates of the specifed point.
        /// </summary>
        /// <param name="point">A point, in pixels, relative to the top left corner of the control.</param>
        /// <returns>A Point filled with the Latitude (Y) and Longitude (X) of the specifide point.</returns>
        public Point GetLocation(Point point)
        {
            return new Point
            {
                X = _tileGenerator.GetLongitude((_offsetX.Pixels + point.X) / TileGenerator.TileSize, Zoom),
                Y = _tileGenerator.GetLatitude((_offsetY.Pixels + point.Y) / TileGenerator.TileSize, Zoom)
            };
        }

        /// <summary>
        /// Tries to capture the mouse to enable dragging of the map.
        /// </summary>
        /// <param name="e">The MouseButtonEventArgs that contains the event data.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // Make sure we get the keyboard.
            Focus();
            if (CaptureMouse())
            {
                _mouseCaptured = true;
                _previousMouse = e.GetPosition(null);
            }
        }

        /// <summary>
        /// Releases the mouse capture and stops dragging of the map.
        /// </summary>
        /// <param name="e">The MouseButtonEventArgs that contains the event data.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
            _mouseCaptured = false;
        }

        /// <summary>
        /// Drags the map, if the mouse was succesfully captured.
        /// </summary>
        /// <param name="e">The MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouseCaptured)
            {
                BeginUpdate();
                Point position = e.GetPosition(null);
                _offsetX.Translate(position.X - _previousMouse.X);
                _offsetY.Translate(position.Y - _previousMouse.Y);
                _previousMouse = position;
                EndUpdate();
            }
        }

        /// <summary>
        /// Alters the zoom of the map, maintaing the same point underneath the mouse at the new zoom level.
        /// </summary>
        /// <param name="e">The MouseWheelEventArgs that contains the event data.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            int newZoom = TileGenerator.GetValidZoom(Zoom + (e.Delta / Mouse.MouseWheelDeltaForOneLine));
            Point mouse = e.GetPosition(this);

            BeginUpdate();
            _offsetX.ChangeZoom(newZoom, mouse.X);
            _offsetY.ChangeZoom(newZoom, mouse.Y);
            // Set this after we've altered the offsets.
            Zoom = newZoom;
            EndUpdate();
        }

        /// <summary>
        /// Notifies child controls that the size has changed.
        /// </summary>
        /// <param name="sizeInfo">
        /// The packaged parameters (SizeChangedInfo), which includes old and new sizes, and which dimension actually changes.
        /// </param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            BeginUpdate();
            _offsetX.ChangeSize(sizeInfo.NewSize.Width);
            _offsetY.ChangeSize(sizeInfo.NewSize.Height);
            _tilePanel.Width = sizeInfo.NewSize.Width;
            _tilePanel.Height = sizeInfo.NewSize.Height;
            EndUpdate();
        }

        private static void OnLatitudeLongitudePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Search for a MapControl parent.
            MapCanvas canvas = null;
            FrameworkElement child = d as FrameworkElement;
            while (child != null)
            {
                canvas = child as MapCanvas;
                if (canvas != null)
                {
                    break;
                }
                child = child.Parent as FrameworkElement;
            }
            canvas?.RepositionChildren();
        }

        private static void OnZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MapCanvas)d).OnZoomChanged();
        }

        private static object OnZoomPropertyCoerceValue(DependencyObject d, object baseValue)
        {
            return TileGenerator.GetValidZoom((int)baseValue);
        }

        private void OnOffsetChanged(object sender, EventArgs e)
        {
            BeginUpdate();
            MapOffset offset = (MapOffset)sender;
            offset.Property.SetValue(_translate, offset.Offset, null);
            EndUpdate();
        }

        private void OnZoomChanged()
        {
            BeginUpdate();
            _offsetX.ChangeZoom(Zoom, ActualWidth / 2.0);
            _offsetY.ChangeZoom(Zoom, ActualHeight / 2.0);
            _tilePanel.Zoom = Zoom;
            EndUpdate();
        }

        private void BeginUpdate()
        {
            _updateCount++;
        }

        private void EndUpdate()
        {
            System.Diagnostics.Debug.Assert(_updateCount != 0, "Must call BeginUpdate first");
            if (--_updateCount == 0)
            {
                _tilePanel.LeftTile = _offsetX.Tile;
                _tilePanel.TopTile = _offsetY.Tile;
                if (_tilePanel.RequiresUpdate)
                {
                    // Display a pretty picture while we play with the tiles.
                    _cache.Visibility = Visibility.Visible;
                    // This will block our thread for a while (UI events will still be processed).
                    _tilePanel.Update();
                    RepositionChildren();
                    _cache.Visibility = Visibility.Hidden;
                    // Save our image for later.
                    _cache.Source = CreateImage();
                }

                // Update viewport.
                Point topleft = GetLocation(new Point(0, 0));
                Point bottomRight = GetLocation(new Point(ActualWidth, ActualHeight));
                Viewport = new Rect(topleft, bottomRight);
            }
        }

        private void RepositionChildren()
        {
            foreach (UIElement element in Children)
            {
                double latitude = GetLatitude(element);
                double longitude = GetLongitude(element);
                if (!double.IsPositiveInfinity(latitude) && !double.IsPositiveInfinity(longitude))
                {
                    double x = (_tileGenerator.GetTileX(longitude, Zoom) - _offsetX.Tile) * TileGenerator.TileSize;
                    double y = (_tileGenerator.GetTileY(latitude, Zoom) - _offsetY.Tile) * TileGenerator.TileSize;
                    SetLeft(element, x);
                    SetTop(element, y);
                    element.RenderTransform = _translate;
                }
            }
        }
    }
}
