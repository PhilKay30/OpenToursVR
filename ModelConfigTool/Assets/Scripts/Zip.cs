/// File: Zip.cs
/// Project:
/// Programmers: The Weeping Angels
/// First Version:
/// Description: Compression/Decompression methods (see coding reference below).
/// 
/// REFERENCE: This code came from a public GitHub by Valeriu Lacatusu found here:
///            https://github.com/lvaleriu/GridComputing/blob/master/Source/GridSharedLibs/CompressionUtils.cs


using System;
using System.IO;
using System.IO.Compression;

public static class Zip
{
    /// <summary>
    /// This method zips a file
    /// </summary>
    /// <param name="sDir">source dir</param>
    /// <param name="sRelativePath">relative path to src dir</param>
    /// <param name="zipStream">zip stream to use for zipping</param>
    public static void CompressFile(string sDir, string sRelativePath, GZipStream zipStream)
    {
        //Compress file name
        char[] chars = sRelativePath.ToCharArray();
        zipStream.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
        foreach (char c in chars)
            zipStream.Write(BitConverter.GetBytes(c), 0, sizeof(char));

        //Compress file content
        byte[] bytes = File.ReadAllBytes(Path.Combine(sDir, sRelativePath));
        zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
        zipStream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// This method zips a directory
    /// </summary>
    /// <param name="sInDir">directory to sip</param>
    /// <param name="sOutFile">path/name of zip file to create</param>
    public static void CompressDirectory(string sInDir, string sOutFile)
    {
        string[] sFiles = Directory.GetFiles(sInDir, "*.*", SearchOption.AllDirectories);
        int iDirLen = sInDir[sInDir.Length - 1] == Path.DirectorySeparatorChar ? sInDir.Length : sInDir.Length + 1;

        using (FileStream outFile = new FileStream(sOutFile, FileMode.Create, FileAccess.Write, FileShare.None))
        using (GZipStream str = new GZipStream(outFile, CompressionMode.Compress))
            foreach (string sFilePath in sFiles)
            {
                string sRelativePath = sFilePath.Substring(iDirLen);
                CompressFile(sInDir, sRelativePath, str);
            }
    }
}
