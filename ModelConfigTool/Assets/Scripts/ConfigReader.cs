/// File: ConfigReader.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: March 20th, 2020
/// Description: This file contains button handlers for the UI

using System;
using System.IO;
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
        doc.Load(@"..\..\Config\config.xml");
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
