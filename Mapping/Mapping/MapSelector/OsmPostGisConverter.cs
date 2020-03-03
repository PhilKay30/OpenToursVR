using Mapping.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace Mapping.MapSelector
{
    /// <summary>
    /// Helper class to handle retrieving data from OSM.
    /// Created by Brendan Brading.
    /// </summary>
    public class OsmPostGisConverter
    {
        private readonly ConnectionData mConnectionData = new ConnectionData();

        /// <summary>
        /// Constructor.
        /// </summary>
        public OsmPostGisConverter()
        {
            // Get reference to config file
            string configPath = FileIO.GetConfigDirectory() + "\\config.xml";

            // Iterate through key-value pairs in config and load the options into the connection data
            // TODO : Abstract this out and make it a specific config-reader class.
            //      The connection data object should call that directly,
            //      and this class should only talk to the connection data object
            foreach ((string key, string value) in ReadConfig(configPath))
            {
                switch (key)
                {
                    case "database":
                        {
                            mConnectionData.DatabaseName = value;
                            break;
                        }
                    case "username":
                        {
                            mConnectionData.Username = value;
                            break;
                        }
                    case "host":
                        {
                            mConnectionData.Host = value;
                            break;
                        }
                    case "password":
                        {
                            mConnectionData.Password = value;
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Converts the data from the map.osm file into PostGis data in the database.
        /// </summary>
        /// <returns>Null if data was converted and added successfully; false otherwise</returns>
        public string ConvertOsmToPostGis()
        {
            // Initialize process to convert OSM data to PostGis data
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = FileIO.GetOsm2PgsqlDirectory() + "\\osm2pgsql.exe",
                    Arguments = new StringBuilder()
                        .Append("--create --password")
                        .Append(" --database ").Append(mConnectionData.DatabaseName)
                        .Append(" --username ").Append(mConnectionData.Username)
                        .Append(" --host ").Append(mConnectionData.Host)
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
                return "Something went wrong while adding the map to the database.";
            }

            // Get reference to map.osm file
            string osmFile = FileIO.GetOutputDirectory() + "\\" + OpenStreetMapInterface.FILE_NAME;

            // Make sure map.osm file exists
            if (!File.Exists(osmFile))
            {
                return "The OSM file does not exist.";
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
                return "Database had issues.";
            }

            // Add the bounds to the database
            databaseInterface.AddBounds(
                boundsNode.Attributes["minlon"].InnerText,
                boundsNode.Attributes["maxlon"].InnerText,
                boundsNode.Attributes["minlat"].InnerText,
                boundsNode.Attributes["maxlat"].InnerText);

            return null;
        }

        /// <summary>
        /// Reads the config file into a series of key-value pairs.
        /// TODO : This should be abstracted out. See comments in constructor.
        /// </summary>
        /// <param name="config">The path to the config file</param>
        /// <returns>An enumeration of key-value pairs from the config file</returns>
        private static IEnumerable<KeyValuePair<string, string>> ReadConfig(string config)
        {
            // Open config file to read elements
            XmlDocument xml = new XmlDocument();
            xml.Load(config);

            // Get the root config node
            XmlNode configNode = xml.DocumentElement;

            // Iterate through the config nodes and return key-value pairs
            foreach (XmlNode node in configNode.ChildNodes)
            {
                string key = node.Name;
                string value = node.InnerXml;
                yield return new KeyValuePair<string, string>(key, value);
            }
        }
    }
}
