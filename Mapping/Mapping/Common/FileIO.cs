using System.IO;
using System.Linq;

namespace Mapping.Common
{
    /// <summary>
    /// Storage class for various directory references.
    /// Created by Timothy J Cowen.
    /// </summary>
    internal static class FileIO
    {
        /// <summary>
        /// Finds the base directory in the codebase.
        /// </summary>
        /// <returns>The base directory</returns>
        private static string GetBaseDirectory()
        {
            // Get the current directory
            string directory = Directory.GetCurrentDirectory();

            // Backtrack through the directory structure until the "Tools" sub-directory is found
            while (directory.Length > 0 && !Directory.GetDirectories(directory).Contains(directory + "\\Tools"))
            {
                int sub = directory.LastIndexOf('\\');
                directory = sub < 0 ? string.Empty : directory.Substring(0, sub);
            }

            // Return the current directory as the "base" directory
            return directory;
        }

        /// <summary>
        /// Finds the osm2pgsql directory in the codebase.
        /// </summary>
        /// <returns>The osm2pgsql directory</returns>
        public static string GetOsm2PgsqlDirectory()
        {
            return GetBaseDirectory() + "\\Tools\\osm2pgsql";
        }

        /// <summary>
        /// Finds the output directory in the codebase.
        /// Creates it if it doesn't exist.
        /// </summary>
        /// <returns>The output directory</returns>
        public static string GetOutputDirectory()
        {
            string dir = GetBaseDirectory() + "\\Output";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }

        /// <summary>
        /// Finds the config directory in the codebase.
        /// </summary>
        /// <returns>The config directory</returns>
        public static string GetConfigDirectory()
        {
            return GetBaseDirectory() + "\\Config";
        }
    }
}
