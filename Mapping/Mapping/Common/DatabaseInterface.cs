using Mapping.SvgConverter;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Windows.Devices.Bluetooth.Background;

// ReSharper disable StringIndexOfIsCultureSpecific.1
namespace Mapping.Common
{
    /// <summary>
    /// Helper class to handle connecting to and querying the database.
    /// Created by Timothy J Cowen.
    /// </summary>
    internal class DatabaseInterface
    {
        private readonly NpgsqlConnection mConnection;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DatabaseInterface()
        {
            // Get connection data
            ConnectionData connectionData = new ConnectionData();

            // Initialize connection data with default values
            // TODO : Read these from a config file!
            connectionData.DebugInit();

            // Initialize the database connection
            mConnection = new NpgsqlConnection(connectionData.ToString());
        }

        /// <summary>
        /// Initializes the database connection by attempting to open it.
        /// </summary>
        /// <returns>True if connection opened successfully; false otherwise</returns>
        public bool Init()
        {
            try
            {
                // Attempt to open the database connection
                mConnection.Open();
                return true;
            }
            catch (IOException)
            {
                MessageBox.Show("The operation threw an IOException for some unknown reason.");
            }
            catch (SocketException)
            {
                MessageBox.Show("The operation threw a SocketException because why not?");
            }
            catch (TimeoutException)
            {
                MessageBox.Show("The operation timed out before a connection to the database could be made.");
            }

            return false;
        }

        /// <summary>
        /// Adds the user-selected bounds to the database for reference in the future
        /// </summary>
        /// <param name="minLon">The minimum longitudinal value</param>
        /// <param name="maxLon">The maximum longitudinal value</param>
        /// <param name="minLat">The minimum latitudinal value</param>
        /// <param name="maxLat">The maximum latitudinal value</param>
        public void AddBounds(string minLon, string maxLon, string minLat, string maxLat)
        {
            // Ensure that the database connection is open
            if (mConnection.FullState != ConnectionState.Open)
            {
                return;
            }

            // This SQL does the following:
            //      Deletes the 'bounds' table if it exists in the database.
            //          This is to make sure that the table is completely clear and will follow our specified rules.
            //      Creates the 'bounds' table if it does not exist in the database (which it shouldn't).
            //          This is to make sure that the table is what we need it to be in the database.
            //      Inserts the boundaries as selected by the user.
            //          This is in a specific format.
            StringBuilder builder = new StringBuilder()
                .Append("DROP TABLE IF EXISTS bounds;")
                .Append(Environment.NewLine)
                .Append("CREATE TABLE IF NOT EXISTS bounds")
                .Append("(id serial, name varchar UNIQUE, geom geometry(Point, 4326));")
                .Append("INSERT INTO bounds (name, geom) VALUES")
                .Append(Environment.NewLine)
                .Append("('top_left', ST_GeomFromText('POINT(")
                .Append(minLon)
                .Append(" ")
                .Append(maxLat)
                .Append(")', 4326)),")
                .Append(Environment.NewLine)
                .Append("('bottom_right', ST_GeomFromText('POINT(")
                .Append(maxLon)
                .Append(" ")
                .Append(minLat)
                .Append(")', 4326));");

            // Execute the command in the database.
            new NpgsqlCommand(builder.ToString(), mConnection).ExecuteNonQuery();
        }


        /// <summary>
        /// Retrieves the bounds as points.
        /// </summary>
        /// <param name="topLeftPoint">The point which will represent the top-left of the bounds</param>
        /// <param name="bottomRightPoint">The point which will represent the bottom-right of the bounds</param>
        public void GetBounds(PostGisPoint topLeftPoint, PostGisPoint bottomRightPoint)
        {
            // Ensure that the database connection is open
            if (mConnection.FullState != ConnectionState.Open)
            {
                return;
            }

            // Error catching
            try
            {
                // Initialize the command
                using NpgsqlCommand command = new NpgsqlCommand(
                    "SELECT name, ST_AsText(ST_Transform(geom, 3857)) AS point FROM bounds;",
                    mConnection);

                // Execute the command and start reading the results
                using NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    // Get the string representation of the name and point
                    string name = reader.GetString(reader.GetOrdinal("name"));
                    string point = reader.GetString(reader.GetOrdinal("point"));

                    // Check which point was read
                    if (name.Equals("top_left"))
                    {
                        // Parse top left point
                        topLeftPoint.X = double.Parse(point[(point.IndexOf("(") + 1)..point.IndexOf(" ")]);
                        topLeftPoint.Y = double.Parse(point[(point.IndexOf(" ") + 1)..point.IndexOf(")")]);
                    }
                    else if (name.Equals("bottom_right"))
                    {
                        // Parse bottom right point
                        bottomRightPoint.X = double.Parse(point[(point.IndexOf("(") + 1)..point.IndexOf(" ")]);
                        bottomRightPoint.Y = double.Parse(point[(point.IndexOf(" ") + 1)..point.IndexOf(")")]);
                    }
                }

                // Normalize the Y values
                topLeftPoint.Y *= -1;
                bottomRightPoint.Y *= -1;
            }
            catch (NpgsqlException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Requests and retrieves the polygons from the database.
        /// </summary>
        /// <param name="topLeftPoint">The top-left bound</param>
        /// <returns>An enumeration of polygons</returns>
        public IEnumerable<PostGisData> GetPolygons(PostGisPoint topLeftPoint = null)
        {
            // Query and return the requested data
            return GetData(
                "SELECT aerialway, aeroway, amenity, landuse, leisure, sport, tourism, water, waterway, " +
                "ST_AsSVG(way,1) AS way FROM planet_osm_polygon;",
                PostGisData.DataType.Polygon, topLeftPoint);
        }

        /// <summary>
        /// Requests and retrieves the roads from the database.
        /// </summary>
        /// <param name="topLeftPoint">The top-left bound</param>
        /// <returns>An enumeration of roads</returns>
        public IEnumerable<PostGisData> GetRoads(PostGisPoint topLeftPoint = null)
        {
            // Query and return the requested data
            return GetData(
                "SELECT bicycle, bridge, highway, public_transport, railway, ST_AsSVG(way,1) AS way " +
                "FROM planet_osm_roads;",
                PostGisData.DataType.Road, topLeftPoint);
        }

        /// <summary>
        /// Requests and retrieves the lines from the database.
        /// </summary>
        /// <param name="topLeftPoint">The top-left bound</param>
        /// <returns>An enumeration of lines</returns>
        public IEnumerable<PostGisData> GetLines(PostGisPoint topLeftPoint = null)
        {
            // Query and return the requested data
            return GetData(
                "SELECT bicycle, bridge, highway, public_transport, railway, ST_AsSVG(way,1) AS way " +
                "FROM planet_osm_line WHERE highway <> '';",
                PostGisData.DataType.Line, topLeftPoint);
        }

        /// <summary>
        /// Requests and retrieves the points from the database.
        /// </summary>
        /// <param name="topLeftPoint">The top-left bound</param>
        /// <returns>An enumeration of points</returns>
        public IEnumerable<PostGisData> GetPoints(PostGisPoint topLeftPoint = null)
        {
            // Query and return the requested data
            return GetData(
                "SELECT amenity, shop, tourism, ST_AsSVG(way,1) AS way FROM planet_osm_point;",
                PostGisData.DataType.Point, topLeftPoint);
        }

        /// <summary>
        /// Retrieves the specified geometries.
        /// </summary>
        /// <param name="query">The query string</param>
        /// <param name="dataType">The type of data being requested</param>
        /// <param name="topLeftPoint">The top-left point of the bounds</param>
        /// <returns></returns>
        private IEnumerable<PostGisData> GetData(string query, PostGisData.DataType dataType, PostGisPoint topLeftPoint = null)
        {
            // Ensure that the database connection is open
            if (mConnection.FullState != ConnectionState.Open)
            {
                yield break;
            }

            // Initialize the command
            using NpgsqlCommand command = new NpgsqlCommand(query, mConnection);

            // Execute the command and start reading the results
            using NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                // Read the data into the object
                PostGisData postGisData = new PostGisData(reader, dataType);

                // Ensure that the location (way) data is valid
                postGisData.Data["way"] = postGisData.Data["way"].Trim();
                if (postGisData.Data["way"] == null || !postGisData.Data["way"].StartsWith('M'))
                {
                    continue;
                }

                // Normalize the location (way) data using the top-left point
                postGisData.Data["way"] = NormalizeOffset(postGisData.Data["way"], topLeftPoint);
                yield return postGisData;
            }
        }

        /// <summary>
        /// Uses the top-left bound to move the retrieved data to a normalized location.
        /// </summary>
        /// <param name="wayData">The location (way) data string</param>
        /// <param name="topLeftPoint">The top-left point of the bounds</param>
        /// <returns>The normalized location (way) data string</returns>
        private string NormalizeOffset(string wayData, PostGisPoint topLeftPoint)
        {
            // If there is no bound, don't touch the data
            if (topLeftPoint == null)
            {
                return wayData;
            }

            // Split up the location data into its individual elements
            string[] wayElements = wayData.Split(' ');

            // Iterate through the elements until the first relevant piece of data is found
            int i = 0;
            while (!double.TryParse(wayElements[i], out _))
            {
                i++;
            }

            // Parse the relevant elements and offset them by the specified amount
            wayElements[i] = (double.Parse(wayElements[i]) - topLeftPoint.X).ToString();
            wayElements[i + 1] = (double.Parse(wayElements[i + 1]) - topLeftPoint.Y).ToString();

            // Concatenate the elements together again
            StringBuilder builder = new StringBuilder();
            foreach (string wayElement in wayElements)
            {
                builder.Append(wayElement).Append(' ');
            }

            return builder.ToString();
        }
    }
}
