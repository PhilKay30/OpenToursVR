using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace Mapping.Common
{
    /// <summary>
    /// Helper class to handle generating and parsing the configuration of the application.
    /// Created by Timothy J Cowen.
    /// </summary>
    internal static class ConfigInterface
    {
        public static string MapServiceToken { get; private set; }
        public static DatabaseConnection ConnectionDb { get; private set; }
        public static ApiConnection ConnectionApi { get; private set; }
        private static ConfigStatus Status { get; set; } = ConfigStatus.Unknown;

        /// <summary>
        /// Reads the configuration file, validates it, and sets up the static objects.
        /// </summary>
        /// <returns>True if the configuration file was loaded successfully; false otherwise</returns>
        public static ConfigStatus LoadConfig()
        {
            // If the configuration was already loaded, do nothing
            if (Status != ConfigStatus.Unknown)
            {
                return Status;
            }

            // Get a reference to the location of the config file
            string configPath = FileIO.GetConfigDirectory() + "\\config.xml";

            // Check that the configuration file exists
            if (File.Exists(configPath))
            {
                try
                {
                    // Load the configuration file in
                    XmlDocument config = new XmlDocument();
                    config.Load(configPath);

                    // Ensure that the first element in the config file is '<config>'
                    if (!config.DocumentElement.Name.Equals("config"))
                    {
                        Status = ConfigStatus.InvalidBaseTag;
                        return Status;
                    }

                    // Ensure that the database node exists
                    if (!config.DocumentElement.Contains("database"))
                    {
                        Status = ConfigStatus.DatabaseTagDoesNotExist;
                        return Status;
                    }

                    // Ensure that the database configuration is valid
                    XmlNode databaseNode = config.GetElementsByTagName("database").Item(0);
                    ConfigStatus dbStatus = ValidateDatabaseNode(databaseNode);
                    if (dbStatus != ConfigStatus.OK)
                    {
                        Status = dbStatus;
                        return Status;
                    }

                    // Ensure that the api node exists
                    if (!config.DocumentElement.Contains("api"))
                    {
                        Status = ConfigStatus.ApiTagDoesNotExist;
                        return Status;
                    }

                    // Ensure that the api configuration is valid
                    XmlNode apiNode = config.GetElementsByTagName("api").Item(0);
                    ConfigStatus apiStatus = ValidateApiNode(apiNode);
                    if (apiStatus != ConfigStatus.OK)
                    {
                        Status = apiStatus;
                        return Status;
                    }

                    // Ensure that the map node exists
                    if (!config.DocumentElement.Contains("map"))
                    {
                        Status = ConfigStatus.MapTagDoesNotExist;
                        return Status;
                    }

                    // Ensure that the map configuration is valid
                    XmlNode mapNode = config.GetElementsByTagName("map").Item(0);
                    ConfigStatus mapStatus = ValidateMapNode(mapNode);
                    if (mapStatus != ConfigStatus.OK)
                    {
                        Status = mapStatus;
                        return Status;
                    }
                }
                catch (XmlException)
                {
                    // An error occurred trying to parse the XML
                    Status = ConfigStatus.ErrorLoadingConfig;
                    return Status;
                }
            }
            else
            {
                // The file does not exist
                Status = ConfigStatus.ConfigDoesNotExist;
                return Status;
            }

            // Everything is set up correctly, so continue
            Status = ConfigStatus.OK;
            return Status;
        }

        /// <summary>
        /// Creates an empty configuration file for the user to fill in.
        /// </summary>
        /// <param name="path">The location to generate the config file</param>
        public static void GenerateEmptyConfigFile(string path)
        {
            StringBuilder builder = new StringBuilder()
                .AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                .AppendLine("<config>")
                .AppendLine("\t<database>")
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentDatabaseHost"), "-->\n")
                .AppendLine("\t\t<host>127.0.0.1</host>")
                .AppendLine()
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentDatabasePort"), "-->\n")
                .AppendLine("\t\t<port>5432</port>")
                .AppendLine()
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentDatabaseName"), "-->\n")
                .AppendLine("\t\t<name>databaseName</name>")
                .AppendLine()
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentDatabaseUser"), "-->\n")
                .AppendLine("\t\t<user>databaseUser</user>")
                .AppendLine()
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentDatabasePass"), "-->\n")
                .AppendLine("\t\t<pass>databasePass</pass>")
                .AppendLine()
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentDatabaseConnectionTimeout"), "-->\n")
                .AppendLine("\t\t<connectionTimeout>10</connectionTimeout>")
                .AppendLine()
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentDatabaseCommandTimeout"), "-->\n")
                .AppendLine("\t\t<commandTimeout>10</commandTimeout>")
                .AppendLine("\t</database>")
                .AppendLine("\t<api>")
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentApiUrl"), "-->\n")
                .AppendLine("\t\t<url>127.0.0.1</url>")
                .AppendLine("\t</api>")
                .AppendLine("\t<map>")
                .AppendLine("\t\t<!-- https://docs.microsoft.com/en-us/bingmaps/getting-started/bing-maps-dev-center-help/getting-a-bing-maps-key -->")
                .AppendJoin(' ', "\t\t<!--", Application.Current.FindResource("ConfigCommentMapServiceToken"), "-->\n")
                .AppendLine("\t\t<serviceToken></serviceToken>")
                .AppendLine("\t</map>")
                .Append("</config>");

            using StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(builder.ToString());
        }

        /// <summary>
        /// Ensures that the database configuration is valid, and is filled in correctly.
        /// </summary>
        /// <param name="node">A reference to the database node in the file</param>
        /// <returns>True if the database configuration is valid; false otherwise</returns>
        private static ConfigStatus ValidateDatabaseNode(XmlNode node)
        {
            // Ensure specified child nodes exist
            if (!node.Contains("host", "port", "name", "user", "pass", "connectionTimeout", "commandTimeout"))
            {
                return ConfigStatus.DatabaseTagIsMissingField;
            }

            // Read each field
            string host = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("host")).InnerText;
            string port = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("port")).InnerText;
            string name = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("name")).InnerText;
            string user = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("user")).InnerText;
            string pass = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("pass")).InnerText;
            string connectionTimeout = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("connectionTimeout")).InnerText;
            string commandTimeout = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("commandTimeout")).InnerText;

            // Ensure each field contains a value
            if (string.IsNullOrWhiteSpace(host)
                || string.IsNullOrWhiteSpace(port)
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(user)
                || string.IsNullOrWhiteSpace(pass)
                || string.IsNullOrWhiteSpace(connectionTimeout)
                || string.IsNullOrWhiteSpace(commandTimeout)
                )
            {
                return ConfigStatus.DatabaseTagIsMissingField;
            }

            // Set up connection object
            ConnectionDb = new DatabaseConnection(host, port, name, user, pass, connectionTimeout, commandTimeout);

            return ConfigStatus.OK;
        }

        /// <summary>
        /// Ensures that the API configuration is valid, and is filled in correctly.
        /// </summary>
        /// <param name="node">A reference to the API node in the file</param>
        /// <returns>True if the API configuration is valid; false otherwise</returns>
        private static ConfigStatus ValidateApiNode(XmlNode node)
        {
            // Ensure specified child node exists
            if (!node.Contains("url"))
            {
                return ConfigStatus.ApiTagIsMissingField;
            }

            // Read field
            string url = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("url")).InnerText;

            if (string.IsNullOrWhiteSpace(url))
            {
                return ConfigStatus.ApiTagIsMissingField;
            }

            // Set up connection object
            ConnectionApi = new ApiConnection(url);

            return ConfigStatus.OK;
        }

        /// <summary>
        /// Ensures that the map configuration is valid, and is filled in correctly.
        /// </summary>
        /// <param name="node">A reference to the map node in the file</param>
        /// <returns>True if the map configuration is valid; false otherwise</returns>
        private static ConfigStatus ValidateMapNode(XmlNode node)
        {
            // Ensure specified child node exists
            if (!node.Contains("serviceToken"))
            {
                return ConfigStatus.MapTagIsMissingField;
            }

            // Read field
            string serviceToken = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("serviceToken")).InnerText;

            if (string.IsNullOrWhiteSpace(serviceToken))
            {
                return ConfigStatus.MapTagIsMissingField;
            }

            // Set up connection object
            MapServiceToken = serviceToken;

            return ConfigStatus.OK;
        }

        /// <summary>
        /// Possible states of the loaded configuration.
        /// </summary>
        internal enum ConfigStatus
        {
            ConfigDoesNotExist,
            ErrorLoadingConfig,
            InvalidBaseTag,
            DatabaseTagDoesNotExist,
            DatabaseTagIsMissingField,
            ApiTagDoesNotExist,
            ApiTagIsMissingField,
            MapTagDoesNotExist,
            MapTagIsMissingField,
            OK,
            Unknown
        }
    }
}
