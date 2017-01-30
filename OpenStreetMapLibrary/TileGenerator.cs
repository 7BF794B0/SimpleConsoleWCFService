using System;
using System.Globalization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OpenStreetMapLibrary
{
    /// <summary>
    /// Helper methods to retrieve information from openstreetmap.org
    /// </summary>
    /// <remarks>See http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames for reference.</remarks>
    class TileGenerator
    {
        private readonly BitmapStore _bitmapStore = new BitmapStore();
        private int _maxDownload = -1;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public TileGenerator()
        {
            _bitmapStore.DownloadCountChanged += OnDownloadCountChanged;
        }

        /// <summary>
        /// The maximum allowed zoom level.
        /// </summary>
        public const int MaxZoom = 18;

        /// <summary>
        /// The size of a tile in pixels.
        /// </summary>
        internal const double TileSize = 256;

        private const string TileFormat = @"http://tile.openstreetmap.org/{0}/{1}/{2}.png";

        private void OnDownloadCountChanged(object sender, EventArgs e)
        {
            if (_dispatcher.Thread != Thread.CurrentThread)
            {
                _dispatcher.BeginInvoke(new Action(() => OnDownloadCountChanged(sender, e)), null);
                return;
            }
            if (DownloadCount == 0)
            {
                _maxDownload = -1;
            }
            else
            {
                if (_maxDownload < DownloadCount)
                {
                    _maxDownload = DownloadCount;
                }
            }
        }

        /// <summary>
        /// Occurs when there is an error downloading a Tile.
        /// </summary>
        public event EventHandler DownloadError
        {
            add { _bitmapStore.DownloadError += value; }
            remove { _bitmapStore.DownloadError -= value; }
        }

        /// <summary>
        /// Gets the number of Tiles requested to be downloaded.
        /// </summary>
        /// <remarks>This is not the number of active downloads.</remarks>
        public int DownloadCount => _bitmapStore.DownloadCount;

        /// <summary>
        /// Returns a valid value for the specified zoom.
        /// </summary>
        /// <param name="zoom">The zoom level to validate.</param>
        /// <returns>A value in the range of 0 - MaxZoom inclusive.</returns>
        public static int GetValidZoom(int zoom)
        {
            return (int)Clip(zoom, 0, MaxZoom);
        }

        /// <summary>
        /// Returns the latitude for the specified tile number.
        /// </summary>
        /// <param name="tileY">The tile number along the Y axis.</param>
        /// <param name="zoom">The zoom level of the tile index.</param>
        /// <returns>A decimal degree for the latitude, limited to aproximately +- 85.0511 degrees.</returns>
        internal double GetLatitude(double tileY, int zoom)
        {
            // n = 2 ^ zoom
            // lat_rad = arctan(sinh(π * (1 - 2 * ytile / n)))
            // lat_deg = lat_rad * 180.0 / π
            double tile = Clip(1 - ((2 * tileY) / Math.Pow(2, zoom)), -1, 1); // Limit value we pass to sinh
            return Math.Atan(Math.Sinh(Math.PI * tile)) * 180.0 / Math.PI;
        }

        /// <summary>
        /// Returns the longitude for the specified tile number.
        /// </summary>
        /// <param name="tileX">The tile number along the X axis.</param>
        /// <param name="zoom">The zoom level of the tile index.</param>
        /// <returns>A decimal degree for the longitude, limited to +- 180 degrees.</returns>
        internal double GetLongitude(double tileX, int zoom)
        {
            // n = 2 ^ zoom
            // lon_deg = xtile / n * 360.0 - 180.0
            double degrees = tileX / Math.Pow(2, zoom) * 360.0;
            return Clip(degrees, 0, 360) - 180.0; // Make sure we limit its range
        }

        /// <summary>
        /// Returns the maximum size, in pixels, for the specifed zoom level.
        /// </summary>
        /// <param name="zoom">The zoom level to calculate the size for.</param>
        /// <returns>The size in pixels.</returns>
        internal double GetSize(int zoom)
        {
            return Math.Pow(2, zoom) * TileSize;
        }

        /// <summary>
        /// Returns the tile index along the X axis for the specified longitude.
        /// </summary>
        /// <param name="longitude">The longitude coordinate.</param>
        /// <param name="zoom">The zoom level of the desired tile index.</param>
        /// <returns>The tile index along the X axis.</returns>
        /// <remarks>The longitude is not checked to be valid and, therefore, the output may not be a valid index.</remarks>
        internal double GetTileX(double longitude, int zoom)
        {
            // n = 2 ^ zoom
            // xtile = ((lon_deg + 180) / 360) * n
            return ((longitude + 180.0) / 360.0) * Math.Pow(2, zoom);
        }

        /// <summary>
        /// Returns the tile index along the Y axis for the specified latitude.
        /// </summary>
        /// <param name="latitude">The latitude coordinate.</param>
        /// <param name="zoom">The zoom level of the desired tile index.</param>
        /// <returns>The tile index along the Y axis.</returns>
        /// <remarks>The latitude is not checked to be valid and, therefore, the output may not be a valid index.</remarks>
        internal double GetTileY(double latitude, int zoom)
        {
            // n = 2 ^ zoom
            // ytile = (1 - (log(tan(lat_rad) + sec(lat_rad)) / π)) / 2 * n
            double radians = latitude * Math.PI / 180.0;
            double log = Math.Log(Math.Tan(radians) + (1.0 / Math.Cos(radians)));
            return (1.0 - (log / Math.PI)) * Math.Pow(2, zoom - 1);
        }

        /// <summary>
        /// Returns a Tile for the specified area.
        /// </summary>
        /// <param name="zoom">The zoom level of the desired tile.</param>
        /// <param name="x">Tile index along the X axis.</param>
        /// <param name="y">Tile index along the Y axis.</param>
        /// <returns>
        /// If any of the indexes are outside the valid range of tile numbers for the specified zoom level,
        /// null will be returned.
        /// </returns>
        internal BitmapImage GetTileImage(int zoom, int x, int y)
        {
            if (string.IsNullOrEmpty(_bitmapStore.CacheFolder))
            {
                throw new InvalidOperationException("Must set the CacheFolder before calling GetTileImage.");
            }

            double tileCount = Math.Pow(2, zoom) - 1;
            if (x < 0 || y < 0 || x > tileCount || y > tileCount) // Bounds check
            {
                return null;
            }

            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, TileFormat, zoom, x, y));
            return _bitmapStore.GetImage(uri);
        }

        /// <summary>
        /// Returns the closest zoom level less than or equal to the specified map size.
        /// </summary>
        /// <param name="size">The size in pixels.</param>
        /// <returns>The closest zoom level for the specified size.</returns>
        internal int GetZoom(double size)
        {
            return (int)Math.Log(size, 2);
        }

        private static double Clip(double value, double minimum, double maximum)
        {
            return value < minimum ? minimum : (value > maximum ? maximum : value);
        }
    }
}
