using System.IO;
using System.Linq;
using System.Xml;

namespace Mapping.Common
{
    /// <summary>
    /// Helper class to handle generating and parsing the configuration of the application.
    /// Created by Timothy J Cowen.
    /// </summary>
    internal static class ConfigInterface
    {
        public static ConnectionData Connection { get; private set; }

        /// <summary>
        /// Reads the configuration file, validates it, and sets up the static objects.
        /// </summary>
        /// <returns>True if the configuration file was loaded successfully; false otherwise</returns>
        public static bool LoadConfig()
        {
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
                        // Document is invalid, so regenerate it and return false
                        // TODO : Add a response to the user
                        File.Delete(configPath);
                        GenerateEmptyConfigFile(configPath);
                        return false;
                    }

                    // Ensure that the database configuration is valid
                    XmlNode databaseNode = config.GetElementsByTagName("database")?.Item(0);
                    if (!ValidateDatabaseNode(databaseNode))
                    {
                        // Database config is invalid
                        // TODO : Add a response to the user
                        return false;
                    }
                }
                catch (XmlException)
                {
                    // Something went wrong while loading the file
                    // TODO : Add a response to the user
                    File.Delete(configPath);
                    GenerateEmptyConfigFile(configPath);
                    return false;
                }
            }
            else
            {
                // Config file does not exist, so generate it
                // TODO : Add a response to the user
                GenerateEmptyConfigFile(configPath);
                return false;
            }

            // Everything is set up correctly, so continue
            return true;
        }

        /// <summary>
        /// Ensures that the database configuration is valid, and is filled in correctly.
        /// </summary>
        /// <param name="node">A reference to the database node in the file</param>
        /// <returns>True if the database configuration is valid; false otherwise</returns>
        private static bool ValidateDatabaseNode(XmlNode node)
        {
            if (!HasChild(node, "host")
                || !HasChild(node, "port")
                || !HasChild(node, "name")
                || !HasChild(node, "user")
                || !HasChild(node, "pass")
                || !HasChild(node, "connectionTimeout")
                || !HasChild(node, "commandTimeout")
                )
            {
                return false;
            }

            string host = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("host")).InnerText;
            string port = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("port")).InnerText;
            string name = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("name")).InnerText;
            string user = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("user")).InnerText;
            string pass = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("pass")).InnerText;
            string connectionTimeout = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("connectionTimeout")).InnerText;
            string commandTimeout = node.ChildNodes.Cast<XmlNode>().Single(child => child.Name.Equals("commandTimeout")).InnerText;

            if (string.IsNullOrWhiteSpace(host)
                || string.IsNullOrWhiteSpace(port)
                || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(user)
                || string.IsNullOrWhiteSpace(pass)
                || string.IsNullOrWhiteSpace(connectionTimeout)
                || string.IsNullOrWhiteSpace(commandTimeout)
                )
            {
                return false;
            }

            Connection = new ConnectionData(host, port, name, user, pass, connectionTimeout, commandTimeout);

            return true;
        }

        /// <summary>
        /// Creates an empty configuration file for the user to fill in.
        /// </summary>
        /// <param name="path">The location to generate the config file</param>
        private static void GenerateEmptyConfigFile(string path)
        {
            using StreamWriter sw = new StreamWriter(path);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<config>");
            sw.WriteLine("\t<database>");
            sw.WriteLine("\t\t<!-- Host to connect to -->");
            sw.WriteLine("\t\t<host>127.0.0.1</host>");
            sw.WriteLine();
            sw.WriteLine("\t\t<!-- Port to connect to -->");
            sw.WriteLine("\t\t<port>5432</port>");
            sw.WriteLine();
            sw.WriteLine("\t\t<!-- Name of database to connect to -->");
            sw.WriteLine("\t\t<name>databaseName</name>");
            sw.WriteLine();
            sw.WriteLine("\t\t<!-- Username of account to connect with -->");
            sw.WriteLine("\t\t<user>databaseUser</user>");
            sw.WriteLine();
            sw.WriteLine("\t\t<!-- Password of account to connect with -->");
            sw.WriteLine("\t\t<pass>databasePass</pass>");
            sw.WriteLine();
            sw.WriteLine("\t\t<!-- Number of seconds to wait for a connection -->");
            sw.WriteLine("\t\t<connectionTimeout>10</connectionTimeout>");
            sw.WriteLine();
            sw.WriteLine("\t\t<!-- Number of seconds to wait for a command to complete -->");
            sw.WriteLine("\t\t<commandTimeout>10</commandTimeout>");
            sw.WriteLine("\t</database>");
            sw.WriteLine("</config>");
        }

        /// <summary>
        /// Checks whether the specified node contains the specified child node.
        /// </summary>
        /// <param name="node">The parent node</param>
        /// <param name="name">The name of the child node to find</param>
        /// <returns></returns>
        private static bool HasChild(XmlNode node, string name)
        {
            return node.ChildNodes.Cast<XmlNode>().Any(childNode => childNode.Name.Equals(name));
        }
    }
}
