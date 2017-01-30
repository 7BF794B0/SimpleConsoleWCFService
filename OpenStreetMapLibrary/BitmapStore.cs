using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Media.Imaging;

namespace OpenStreetMapLibrary
{
    /// <summary>
    /// Manages cacheing and downloading of images from openstreetmap.org
    /// </summary>
    class BitmapStore
    {
        private int _downloadCount;

        /// <summary>
        /// Occurs when the value of DownloadCount has changed.
        /// </summary>
        public event EventHandler DownloadCountChanged;

        /// <summary>
        /// Occurs when there is an error downloading a Tile.
        /// </summary>
        public event EventHandler DownloadError;

        /// <summary>
        /// Gets or sets the folder used to store the downloaded tiles.
        /// </summary>
        /// <remarks>This must be set before any call to GetTileImage.</remarks>
        public readonly string CacheFolder = @"ImageCache";

        /// <summary>
        /// Gets the number of Tiles requested to be downloaded.
        /// </summary>
        /// <remarks>This is not the number of active downloads.</remarks>
        public int DownloadCount => _downloadCount;

        /// <summary>
        /// Gets or sets the user agent used to make the tile request.
        /// </summary>
        /// <remarks>This should be set before any call to GetTileImage.</remarks>
        public string UserAgent { get; set; }

        /// <summary>
        /// Retreieves the image for the specified uri, using the cache if available.
        /// </summary>
        /// <param name="uri">The uri of the file to load.</param>
        /// <returns>
        /// A BitmapImage for the specified uri, or null if an error occured.
        /// </returns>
        public BitmapImage GetImage(Uri uri)
        {
            // Since this is an internal class we don't need to validate the arguments.
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(CacheFolder), "Must set the CacheFolder before calling GetImage.");
            System.Diagnostics.Debug.Assert(uri != null, "Cannot pass in null values.");

            string localName = GetCacheFileName(uri);
            if (File.Exists(localName))
            {
                FileStream file = null;
                try
                {
                    file = File.OpenRead(localName);
                    return GetImageFromStream(file);
                }
                catch (NotSupportedException)
                {
                    // Problem creating the bitmap (file corrupt?)
                }
                catch (IOException)
                {
                    // Or a prolbem opening the file. We'll try to re-download the file.
                }
                finally
                {
                    file?.Dispose();
                }
            }

            // We don't have it in cache of the copy in cache is corrupted. Either
            // way we need download the file.
            return DownloadBitmap(uri);
        }

        private BitmapImage DownloadBitmap(Uri uri)
        {
            Interlocked.Increment(ref _downloadCount);
            RaiseDownloadCountChanged();

            MemoryStream buffer = null;
            try
            {
                // First download the image to our memory.
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = UserAgent;

                buffer = new MemoryStream();
                using (var response = request.GetResponse())
                {
                    var stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        stream.CopyTo(buffer);
                        stream.Close();
                    }
                }

                // Then save a copy for future reference, making sure to rewind
                // the stream to the start.
                buffer.Position = 0;
                SaveCacheImage(buffer, uri);

                // Finally turn the memory into a beautiful picture.
                buffer.Position = 0;
                return GetImageFromStream(buffer);
            }
            catch (WebException)
            {
                RaiseDownloadError();
            }
            catch (NotSupportedException) // Problem creating the bitmap (messed up download?)
            {
                RaiseDownloadError();
            }
            finally
            {
                Interlocked.Decrement(ref _downloadCount);
                RaiseDownloadCountChanged();
                buffer?.Dispose();
            }
            return null;
        }

        private string GetCacheFileName(Uri uri)
        {
            return Path.Combine(CacheFolder, uri.LocalPath.TrimStart('/'));
        }

        private BitmapImage GetImageFromStream(Stream stream)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();

            // Very important - lets us download in one thread and pass it back to the UI.
            bitmap.Freeze();
            return bitmap;
        }

        private void SaveCacheImage(Stream stream, Uri uri)
        {
            string path = GetCacheFileName(uri);
            FileStream file = null;
            try
            {
                if (path != null)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    file = File.Create(path);
                }

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(stream));
                if (file != null)
                    encoder.Save(file);
            }
            catch (IOException)
            {
                // Couldn't save the file.
            }
            finally
            {
                file?.Dispose();
            }
        }

        private void RaiseDownloadCountChanged()
        {
            var callback = DownloadCountChanged;
            callback?.Invoke(null, EventArgs.Empty);
        }

        private void RaiseDownloadError()
        {
            var callback = DownloadError;
            callback?.Invoke(null, EventArgs.Empty);
        }
    }
}
