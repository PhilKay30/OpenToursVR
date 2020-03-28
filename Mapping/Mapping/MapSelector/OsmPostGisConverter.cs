using Mapping.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;

namespace Mapping.MapSelector
{
    /// <summary>
    /// Helper class to handle retrieving data from OSM.
    /// Created by Brendan Brading.
    /// </summary>
    public class OsmPostGisConverter
    {
        /// <summary>
        /// Converts the data from the map.osm file into PostGis data in the database.
        /// </summary>
        /// <returns>Null if data was converted and added successfully; false otherwise</returns>
        public string ConvertOsmToPostGis()
        {
            // Set password environment variable
            Environment.SetEnvironmentVariable("PGPASSWORD", ConfigInterface.ConnectionDb.Password);

            // Initialize process to convert OSM data to PostGis data
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = FileIO.GetOsm2PgsqlDirectory() + "\\osm2pgsql.exe",
                    Arguments = new StringBuilder()
                        .Append("--create")
                        .Append(" --database ").Append(ConfigInterface.ConnectionDb.DatabaseName)
                        .Append(" --username ").Append(ConfigInterface.ConnectionDb.Username)
                        .Append(" --host ").Append(ConfigInterface.ConnectionDb.Host)
                        .Append(" --style ").Append(FileIO.GetOsm2PgsqlDirectory() + "\\default.style")
                        .Append(" ").Append(FileIO.GetOutputDirectory() + "\\" + OpenStreetMapInterface.FILE_NAME)
                        .ToString()
                }
            };

            // Start the process and wait for it to finish
            process.Start();
            process.WaitForExit();

            // Make sure the conversion was successful
            if (process.ExitCode != 0)
            {
                return Application.Current.FindResource("PromptMapOsmToPostGisFailure")?.ToString();
            }

            // Get reference to map.osm file
            string osmFile = FileIO.GetOutputDirectory() + "\\" + OpenStreetMapInterface.FILE_NAME;

            // Make sure map.osm file exists
            if (!File.Exists(osmFile))
            {
                return Application.Current.FindResource("PromptMapNoFile")?.ToString();
            }

            // Open map.osm file to read elements
            XmlDocument xml = new XmlDocument();
            xml.Load(osmFile);

            // Get the specific bounds node
            XmlNode boundsNode = xml.GetElementsByTagName("bounds").Item(0);

            // Initialize the database connection
            DatabaseInterface databaseInterface = new DatabaseInterface();
            if (!databaseInterface.Init())
            {
                return Application.Current.FindResource("PromptMapDatabaseConnectionFailure")?.ToString();
            }

            // Add the bounds to the database
            databaseInterface.AddBounds(
                boundsNode.Attributes["minlon"].InnerText,
                boundsNode.Attributes["maxlon"].InnerText,
                boundsNode.Attributes["minlat"].InnerText,
                boundsNode.Attributes["maxlat"].InnerText);

            return null;
        }
    }
}
