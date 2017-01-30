using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenStreetMapLibrary
{
    class TilePanel : Canvas
    {
        private Tile _baseTile;
        private readonly TileGenerator _tileGenerator;
        private int _columns;
        private int _rows;
        private int _zoom;

        /// <summary>
        /// Initializes a new instance of the TilePanel class.
        /// </summary>
        public TilePanel(TileGenerator tileGenerator)
        {
            _tileGenerator = tileGenerator;
            // This stops the Images from being blurry.
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
        }

        /// <summary>
        /// Gets or sets the tile index at the left edge of the control.
        /// </summary>
        public int LeftTile { get; set; }

        /// <summary>
        /// Gets or sets the tile index at the top edge of the control.
        /// </summary>
        public int TopTile { get; set; }

        /// <summary>
        /// Gets a value indicating whether a call to Update is required.
        /// </summary>
        public bool RequiresUpdate => _baseTile == null ||
                                      _baseTile.TileX + 1 - LeftTile != 0 || // The baseTile is located at -1,-1 but LeftTile/TopTile is 0,0
                                      _baseTile.TileY + 1 - TopTile != 0;

        /// <summary>
        /// Gets or sets the zoom level for the tiles.
        /// </summary>
        public int Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    _baseTile = null; // Force complete refresh
                }
            }
        }

        /// <summary>
        /// Re-arranges the Tiles inside the grid.
        /// </summary>
        /// <remarks>The control will only update itself when RequiresUpdate returns true.</remarks>
        public void Update()
        {
            if (_baseTile == null)
            {
                RegenerateTiles();
            }
            else
            {
                int changeX = _baseTile.TileX + 1 - LeftTile;
                int changeY = _baseTile.TileY + 1 - TopTile;

                if (changeX != 0 || changeY != 0)
                {
                    if (Math.Abs(changeX) > 1 && Math.Abs(changeY) > 1 || Children.Count == 0)
                    {
                        // It's changed too much or we don't have any tiles.
                        RegenerateTiles();
                    }
                    else // Only changed a little.
                    {
                        if (changeX != 0)
                            ChangeColumns(changeX);

                        if (changeY != 0)
                            ChangeRows(changeY);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures the grid has the correct number of rows and columns.
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            int oldColumns = _columns;
            int oldRows = _rows;
            _rows = (int)Math.Ceiling(sizeInfo.NewSize.Height / TileGenerator.TileSize) + 1;
            _columns = (int)Math.Ceiling(sizeInfo.NewSize.Width / TileGenerator.TileSize) + 1;

            // First check if we need to add tiles.
            if (oldColumns < _columns || oldRows < _rows)
            {
                RegenerateTiles();
            }
            else if (oldColumns > _columns || oldRows > _rows) // Don't need to add so we can just trim the columns/rows.
            {
                // Would be easier if we could use the IList<T>.RemoveAll but UIElementCollection doesn't have it.
                for (int i = Children.Count - 1; i >= 0; --i)
                {
                    Tile tile = (Tile)Children[i];
                    if (tile.Column >= _columns || tile.Row >= _rows)
                        Children.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Moves the Tile's column by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to change the column by.</param>
        private void ChangeColumns(int amount)
        {
            ChangeTiles(tile =>
            {
                tile.Column += amount;
                if (tile.Column < -1)
                {
                    tile.Column = _columns - 1;
                    tile.TileX += _columns + 1;
                }
                else if (tile.Column > _columns - 1)
                {
                    tile.Column = -1;
                    tile.TileX -= _columns + 1;
                }
            });
        }

        /// <summary>
        /// Moves the Tile's row by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to change the row by.</param>
        private void ChangeRows(int amount)
        {
            ChangeTiles(tile =>
            {
                tile.Row += amount;
                if (tile.Row < -1)
                {
                    tile.Row = _rows - 1;
                    tile.TileY += _rows + 1;
                }
                else if (tile.Row > _rows - 1)
                {
                    tile.Row = -1;
                    tile.TileY -= _rows + 1;
                }
            });
        }

        /// <summary>
        /// Repositions the Tiles after any changes.
        /// </summary>
        /// <param name="changeTile">Called on every Tile to allow changes to be made to its position.</param>
        private void ChangeTiles(Action<Tile> changeTile)
        {
            // We need something to compare to so set it to the first.
            _baseTile = (Tile)Children[0];
            for (int i = 0; i < Children.Count; ++i)
            {
                Tile tile = (Tile)Children[i];
                changeTile(tile);
                SetLeft(tile, TileGenerator.TileSize * tile.Column);
                SetTop(tile, TileGenerator.TileSize * tile.Row);
                // Find the upper left tile.
                if (tile.TileX <= _baseTile.TileX && tile.TileY <= _baseTile.TileY)
                    _baseTile = tile;
            }
        }

        /// <summary>
        /// Clears and the reloads all the Tiles contained in the control.
        /// </summary>
        private void RegenerateTiles()
        {
            Children.Clear();
            for (int x = -1; x < _columns; ++x)
            {
                for (int y = -1; y < _rows; ++y)
                {
                    Tile tile = new Tile(Zoom, LeftTile + x, TopTile + y, _tileGenerator)
                    {
                        Column = x,
                        Row = y
                    };
                    SetLeft(tile, TileGenerator.TileSize * x);
                    SetTop(tile, TileGenerator.TileSize * y);
                    Children.Add(tile);
                }
            }
            _baseTile = (Tile)Children[0];
        }
    }
}
