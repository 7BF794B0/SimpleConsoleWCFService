using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Windows;

namespace OpenStreetMapLibrary
{
    /// <summary>
    /// Uses nominatim.openstreetmap.org to search for the specified information.
    /// </summary>
    class SearchProvider
    {
        private const string SearchPath = @"http://nominatim.openstreetmap.org/search";
        private readonly List<SearchResult> _results = new List<SearchResult>();

        /// <summary>
        /// Occurs when the search has completed.
        /// </summary>
        public event EventHandler SearchCompleted;

        /// <summary>
        /// Gets the results returned from the most recent search.
        /// </summary>
        public SearchResult[] Results => _results.ToArray();

        /// <summary>
        /// Searches for the specified query in the specified area.
        /// </summary>
        /// <param name="query">The information to search for.</param>
        /// <param name="area">The area to localize results.</param>
        /// <returns>True if search has started, false otherwise.</returns>
        /// <remarks>
        /// The query is first parsed to see if it is a valid coordinate, if not then then a search
        /// is carried out using nominatim.openstreetmap.org. A return valid of false, therefore,
        /// doesn't indicate the method has failed, just that there was no need to perform an online search.
        /// </remarks>
        public bool Search(string query, Rect area)
        {
            query = (query ?? string.Empty).Trim();
            if (query.Length == 0)
            {
                return false;
            }
            if (TryParseLatitudeLongitude(query))
            {
                return false;
            }

            string bounds = string.Format(
                CultureInfo.InvariantCulture,
                "{0:f4},{1:f4},{2:f4},{3:f4}",
                area.Left,
                area.Bottom, // Area is upside down
                area.Right,
                area.Top);

            WebClient client = new WebClient();
            client.QueryString.Add("q", Uri.EscapeDataString(query));
            client.QueryString.Add("viewbox", Uri.EscapeDataString(bounds));
            client.QueryString.Add("format", "xml");
            try
            {
                client.DownloadStringAsync(new Uri(SearchPath));
            }
            catch (WebException) // An error occurred while downloading the resource
            {
                return false;
            }
            return true;
        }

        private void OnSearchCompleted()
        {
            var callback = SearchCompleted;
            callback?.Invoke(this, EventArgs.Empty);
        }

        private bool TryParseLatitudeLongitude(string text)
        {
            string[] tokens = text.Split(new[] { ',', ' ', '°' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 2)
            {
                double latitude, longitude;
                if (double.TryParse(tokens[0], out latitude) && double.TryParse(tokens[1], out longitude))
                {
                    SearchResult result = new SearchResult(1)
                    {
                        DisplayName =
                            string.Format(CultureInfo.CurrentUICulture, "{0:f4}°, {1:f4}°", latitude, longitude),
                        Latitude = latitude,
                        Longitude = longitude
                    };
                    _results.Clear();
                    _results.Add(result);
                    OnSearchCompleted();
                    return true;
                }
            }
            return false;
        }
    }
}
