using Npgsql;
using System.Collections.Generic;

namespace Mapping.SvgConverter
{
    /// <summary>
    /// Storage class to handle the data read in from PostGis.
    /// Created by Timothy J Cowen.
    /// </summary>
    public class PostGisData
    {
        public Dictionary<string, string> Data { get; } = new Dictionary<string, string>();
        public DataType Type { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader">The PostGis reader</param>
        /// <param name="type">The type of data being read (polygon/road/line/point)</param>
        public PostGisData(NpgsqlDataReader reader, DataType type)
        {
            // Set type
            Type = type;

            // Iterate through columns in result
            int numOfCols = reader.FieldCount;
            for (int i = 0; i < numOfCols; i++)
            {
                // Get column name and value
                string column = reader.GetName(i);
                string value = reader.GetValue(reader.GetOrdinal(column)).ToString();

                // Add data if it exists
                if (!string.IsNullOrEmpty(value))
                {
                    Data.Add(column, value);
                }
            }
        }

        /// <summary>
        /// Retrieves the colour of the data depending on what it contains.
        /// </summary>
        /// <returns>The SVG-friendly colour that the data should show as</returns>
        public string GetColour()
        {
            if (Data.ContainsKey("harbour") || Data.ContainsKey("water") || Data.ContainsKey("waterway"))
            {
                return "dodgerblue";
            }

            if (Data.ContainsKey("highway") || Data.ContainsKey("motorcar") || Data.ContainsKey("bicycle"))
            {
                return "black";
            }

            if (Data.ContainsKey("aerialway") || Data.ContainsKey("aeroway") || Data.ContainsKey("public_transport") ||
                Data.ContainsKey("railway"))
            {
                return "lightgreen";
            }

            return "gainsboro";
        }

        /// <summary>
        /// An enumeration of the different data types
        /// </summary>
        public enum DataType
        {
            Polygon,
            Road,
            Line,
            Point
        }
    }
}
