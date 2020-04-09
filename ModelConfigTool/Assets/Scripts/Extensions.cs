/// File: Extensions.cs
/// Project: Paris VR 2.0
/// Programmers: Weeping Angels
/// First Version: April 6th, 2020
/// Description: This file contains conversion and extensions methods

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// This class represents a set of string extensions
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// This extension method strips everything except for hex chars out of a string
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public static string StripToHex(this string inputString)
    {
        Regex rgx = new Regex("[^a-fA-F0-9]");
        return rgx.Replace(inputString, "");
    }

    /// <summary>
    /// This method splits a POINT string into its pieces
    /// </summary>
    /// <param name="point">string to split</param>
    /// <param name="values">OUT param to hold values</param>
    public static void ToPoint(this string point, ref Dictionary<string, double> values)
    {
        int from = point.IndexOf("(") + "(".Length;
        int to = point.LastIndexOf(")");
        point = point.Substring(from, to - from);
        string[] points = point.Split(' ');

        // Here there be magic jazz hands
        values["longitude"] = Convert.ToDouble(points[0]);
        values["latitude"] = Convert.ToDouble(points[1]);
    }
}

/// <summary>
/// This class contains a method to convert a hex string to binary
/// </summary>
public static class Converter
{
    /// <summary>
    /// Converts a hex encoding string into binary data
    /// </summary>
    /// <param name="hexStr">string to convert</param>
    /// <returns>the binary array</returns>
    public static byte[] HexStringToBinary(string hexStr)
    {
        string strBuff = hexStr.StripToHex();
        List<byte> bitey = new List<byte>();
        for (int i = 0; i < strBuff.Length; i++)
        {
            char[] charArr =
            {
                    strBuff[i],
                    strBuff[++i]
            };
            string biteme = new string(charArr);
            byte number = Convert.ToByte(biteme, 16);
            bitey.Add(number);
        }
        return bitey.ToArray();
    }
}
