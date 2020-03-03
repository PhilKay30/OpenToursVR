using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Mapping.Common;

namespace Mapping.MapSelector
{
    /// <summary>
    /// Helper class to handle retrieving data from OSM.
    /// Created by Timothy J Cowen.
    /// </summary>
    public static class OpenStreetMapInterface
    {
        private const string URL_BASE = "https://api.openstreetmap.org/api/";
        private const string URL_RETRIEVE_MAP = URL_BASE + "0.6/map";
        public const string FILE_NAME = "map.osm";

        /// <summary>
        /// Retrieves the map data from OSM.
        /// </summary>
        /// <param name="left">The left-most bound</param>
        /// <param name="bottom">The bottom-most bound</param>
        /// <param name="right">The right-most bound</param>
        /// <param name="top">The top-most bound</param>
        /// <returns>True if data was successfully retrieved; false otherwise</returns>
        public static bool GetMap(string left, string bottom, string right, string top)
        {
            // Get the URL to retrieve data from
            string url = GetUrlWithParams(URL_RETRIEVE_MAP, new KeyValuePair<string, string>(
                        "bbox",
                        left + "," + bottom + "," + right + "," + top));

            // Query for data and get response confirmation
            bool response = QueryForFile(url);
            return response;
        }

        /// <summary>
        /// Generates the appropriate URL for the query
        /// </summary>
        /// <param name="baseUrl">The base URL for the query</param>
        /// <param name="urlParams">The parameters for the query</param>
        /// <returns>The query URL</returns>
        private static string GetUrlWithParams(string baseUrl, params KeyValuePair<string, string>[] urlParams)
        {
            // If no parameters exist, don't touch the URL
            if (urlParams == null || urlParams.Length == 0)
            {
                return baseUrl;
            }

            // Add the query modifier to the base URL
            string modifiedUrl = baseUrl + "?";

            // Iterate through the parameters and add them to the URL
            foreach (KeyValuePair<string, string> urlParam in urlParams)
            {
                if (!modifiedUrl.EndsWith("?"))
                {
                    modifiedUrl += "&";
                }

                modifiedUrl += urlParam.Key + "=" + urlParam.Value;
            }

            return modifiedUrl;
        }

        /// <summary>
        /// Retrieves the requested data and stores it into the file.
        /// </summary>
        /// <param name="url">The query URL</param>
        /// <param name="requestMethod">The type of request</param>
        /// <returns>True if data was successfully retrieved; false otherwise</returns>
        private static bool QueryForFile(string url, string requestMethod = "GET")
        {
            // Initialize the web request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = requestMethod;

            try
            {
                // Initialize the response and read from it
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using Stream stream = response.GetResponseStream();

                // Ensure the stream exists
                if (stream != null)
                {
                    // Initialize stream reader and writer
                    using StreamReader sr = new StreamReader(stream);
                    using StreamWriter sw = new StreamWriter(FileIO.GetOutputDirectory() + "\\" + FILE_NAME);

                    // Read data from the stream and output it to the file
                    string data;
                    while ((data = sr.ReadLine()) != null)
                    {
                        sw.WriteLine(data);
                    }
                }
            }
            catch (WebException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }
    }
}
