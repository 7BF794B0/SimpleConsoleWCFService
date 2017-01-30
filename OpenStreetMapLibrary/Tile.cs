using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenStreetMapLibrary
{
    /// <summary>
    /// Represents a single map tile.
    /// </summary>
    class Tile : Image
    {
        private int _tileX;
        private int _tileY;
        private readonly int _zoom;
        private readonly TileGenerator _tileGenerator;

        /// <summary>
        /// Initializes a new instance of the Tile class.
        /// </summary>
        /// <param name="zoom">The zoom level for the tile.</param>
        /// <param name="x">The tile index along the X axis.</param>
        /// <param name="y">The tile index along the Y axis.</param>
        /// <param name="tileGenerator">TileGenerator</param>
        public Tile(int zoom, int x, int y, TileGenerator tileGenerator)
        {
            _tileX = x;
            _tileY = y;
            _zoom = zoom;
            _tileGenerator = tileGenerator;
            LoadTile();
        }

        /// <summary>
        /// Gets or sets the tile index along the X axis.
        /// </summary>
        public int TileX
        {
            get
            {
                return _tileX;
            }
            set
            {
                if (value != _tileX)
                {
                    _tileX = value;
                    LoadTile();
                }
            }
        }

        /// <summary>
        /// Gets or sets the tile index along the Y axis.
        /// </summary>
        public int TileY
        {
            get
            {
                return _tileY;
            }
            set
            {
                if (value != _tileY)
                {
                    _tileY = value;
                    LoadTile();
                }
            }
        }

        /// <summary>
        /// Gets or sets the column index of the tile.
        /// </summary>
        internal int Column { get; set; }

        /// <summary>
        /// Gets or sets the row index of the tile.
        /// </summary>
        internal int Row { get; set; }

        private void LoadTile()
        {
            Source = null;
            System.Threading.ThreadPool.QueueUserWorkItem(LoadTileInBackground);
        }

        private void LoadTileInBackground(object state)
        {
            ImageSource image = _tileGenerator.GetTileImage(_zoom, _tileX, _tileY);
            // We've already set the Source to null before calling this method.
            if (image != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Source = image;
                }));
            }
        }
    }
}
