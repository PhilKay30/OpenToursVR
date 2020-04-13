/// File: ConfigReader.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: March 20th, 2020
/// Description: This file contains button handlers for the UI

using System;
using System.IO;
using System.Linq;
using System.Xml;

public class ConfigReader
{
    /// <summary>
    /// Get's the API url out of the config file
    /// </summary>
    /// <returns></returns>
    public string GetApiURL()
    {
        string apiURL = "";
        XmlDocument doc = new XmlDocument();
        string configFile = GetConfigFile();
        doc.Load(configFile);
        foreach (XmlNode pNode in doc.DocumentElement.ChildNodes)
        {
            if (pNode.Name == "api")
            {
                foreach (XmlNode cNode in pNode.ChildNodes)
                {
                    if (cNode.Name == "url")
                    {
                        apiURL = cNode.InnerText;
                        break;
                    }
                }
            }
        }
        return apiURL;
    }

    /// <summary>
    /// This method finds the config file
    /// </summary>
    /// <returns>config file address</returns>
    private string GetConfigFile()
    {
        string directory = Directory.GetCurrentDirectory();

        // Backtrack through the directory structure until the "Tools" sub-directory is found
        while (directory.Length > 0 && !Directory.GetDirectories(directory).Contains(directory + "\\Config"))
        {
            int sub = directory.LastIndexOf('\\');
            directory = sub < 0 ? string.Empty : directory.Substring(0, sub);
        }

        // Return the config file path
        return directory + "\\Config\\config.xml";
    }
}
