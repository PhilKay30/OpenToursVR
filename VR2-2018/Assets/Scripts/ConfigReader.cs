/// File: ConfigReader.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: March 20th, 2020
/// Description: This file contains button handlers for the UI

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class ConfigReader
{
    /// <summary>
    /// Name of the config file for this class
    /// </summary>
    private string myConfigFile = "config.xml";

    /// <summary>
    /// Default URL to put in the config file if it's being created from scratch
    /// </summary>
    private string defaultURL = "http://192.0.203.84:5000";

    /// <summary>
    /// Constructor
    /// creates default config file if it doesn't exist
    /// </summary>
    public ConfigReader()
    {
        if (!File.Exists(myConfigFile))
        {
            CreateDefaultConfig(myConfigFile);
        }
    }

    /// <summary>
    /// This method creates a default config file
    /// </summary>
    /// <param name="fileName">name of config file to create</param>
    private void CreateDefaultConfig(string fileName)
    {
        List<string> lines = new List<string>();
        lines.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        lines.Add("<config>");
        lines.Add("\t<api>");
        lines.Add("\t\t<url>" + defaultURL + "</url>");
        lines.Add("\t</api>");
        lines.Add("</config>");
        File.WriteAllLines(fileName, lines.ToArray());
    }

    /// <summary>
    /// Gets the API URL from the config file
    /// </summary>
    /// <returns></returns>
    public string GetApiURL()
    {
        string apiURL = "";
        XmlDocument doc = new XmlDocument();
        doc.Load(myConfigFile);
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
}
