using Mapping.Common;
using Mapping.SvgConverter;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Mapping.ApiHandler
{
    // ReSharper disable StringIndexOfIsCultureSpecific.1
    internal class ApiHandler
    {
        /// <summary>
        /// Inserts the PNG into the Database.
        /// </summary>
        /// <param name="rotation">The rotation of the image</param>
        /// <param name="imagePath">Path of where the image is stored.</param>
        /// <param name="name">Name of the image</param>
        /// <param name="botLeftCorner">The GIS data of the bottom left corner</param>
        /// <param name="widthInKm">The width of the image in kilometers</param>
        /// <param name="heightInKm">The height of the image in kilometers</param>
        public void InsertPng(
            double rotation,
            string imagePath,
            string name,
            PostGisPoint botLeftCorner,
            double widthInKm,
            double heightInKm)
        {
            // Get the bytes from the PNG file and convert to hex string
            byte[] imageBytes = File.ReadAllBytes(FileIO.GetOutputDirectory() + imagePath);
            string imageHex = string.Concat(imageBytes.Select(b => b.ToString("X2")).ToArray());

            // Create the JSON object
            JsonObject obj = new JsonObject
            {
                image_name = name,
                image_data = imageHex,
                image_size = imageHex.Length,
                bottom_left_corner = $"POINT({botLeftCorner.Longitude} {botLeftCorner.Latitude})",
                km_height = heightInKm,
                km_width = widthInKm,
                image_rotation = rotation
            };

            // Serialize for the message
            string jsonString = JsonSerializer.Serialize(obj);

            // Create the API request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigInterface.ConnectionApi.AddImageUrl);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "POST";

            // Prepare the message
            byte[] message = new ASCIIEncoding().GetBytes(jsonString);
            request.ContentLength = message.Length;
            Stream stream = request.GetRequestStream();

            // Send the message
            stream.Write(message, 0, message.Length);

            try
            {
                // Retrieve the API response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show("Something went wrong.\n" + response.StatusDescription);
                }
            }
            catch (WebException e)
            {
                MessageBox.Show("Something went wrong.\n" + e.Message);
            }

            MessageBox.Show("Image was successfully inserted into the database.");
        }

        /// <summary>
        /// TODO: Set this to an API call.
        /// </summary>
        /// <returns>List of KvP of the pairs</returns>
        public List<KeyValuePair<string, PostGisPoint>> GetBounds()
        {
            const string query = "SELECT name, ST_AsText(geom) AS point FROM bounds;";
            List<KeyValuePair<string, PostGisPoint>> points = new List<KeyValuePair<string, PostGisPoint>>();

            using (NpgsqlConnection conn = new NpgsqlConnection(ConfigInterface.ConnectionDb.ToString()))
            {
                using NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                conn.Open();
                try
                {
                    NpgsqlDataReader rsp = cmd.ExecuteReader();
                    while (rsp.Read())
                    {
                        string name = rsp.GetString(rsp.GetOrdinal("name"));
                        string point = rsp.GetString(rsp.GetOrdinal("point"));

                        // Check which point was read
                        if (name.Equals("top_left"))
                        {
                            // Parse top left point
                            PostGisPoint topLeftPoint = new PostGisPoint
                            {
                                X = double.Parse(point[(point.IndexOf("(") + 1)..point.IndexOf(" ")]),
                                Y = double.Parse(point[(point.IndexOf(" ") + 1)..point.IndexOf(")")])
                            };
                            points.Add(new KeyValuePair<string, PostGisPoint>("top_left", topLeftPoint));
                        }
                        // bottom_right corner
                        else if (name.Equals("bottom_right"))
                        {
                            // Parse bottom right point
                            PostGisPoint bottomRightPoint = new PostGisPoint
                            {
                                X = double.Parse(point[(point.IndexOf("(") + 1)..point.IndexOf(" ")]),
                                Y = double.Parse(point[(point.IndexOf(" ") + 1)..point.IndexOf(")")])
                            };
                            points.Add(new KeyValuePair<string, PostGisPoint>("bottom_right", bottomRightPoint));
                        }
                        else
                        {
                            throw new Exception("Database is broken.");
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("There are more than 4 sides to the map.\n" +
                                    "Clearing Data, please start over.");
                }
            }
            return points;
        }

        /// <summary>
        /// Generic class for the JSON object.
        /// These must remain named as they are for consistency with the API.
        /// </summary>
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class JsonObject
        {
            public string image_name { get; set; }
            public string image_data { get; set; }
            public int image_size { get; set; }
            public string bottom_left_corner { get; set; }
            public double km_height { get; set; }
            public double km_width { get; set; }
            public double image_rotation { get; set; }
        }

        /// <summary>
        /// Generic class for the JSON point object.
        /// These must remain named as they are for consistency with the API.
        /// </summary>
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class JsonPoint
        {
            public string point_location { get; set; }
            public string point_name { get; set; }
            public string point_desc { get; set; }
            public string point_image { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <param name="pngPath"></param>
        public bool AddPoint(string point, string name, string desc, string pngPath)
        {
            string imageHex = string.Empty;
            if (!string.IsNullOrEmpty(pngPath))
            {
                byte[] bytes = File.ReadAllBytes(pngPath);
                imageHex = string.Concat(bytes.Select(b => b.ToString("X2")).ToArray());
            }

            JsonPoint obj = new JsonPoint
            {
                point_location = point,
                point_name = name,
                point_desc = desc,
                point_image = imageHex
            };

            string jsonString = JsonSerializer.Serialize(obj);

            // Create the API request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigInterface.ConnectionApi.AddPointUrl);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "POST";

            // Prepare the message
            byte[] message = new ASCIIEncoding().GetBytes(jsonString);
            request.ContentLength = message.Length;
            Stream stream = request.GetRequestStream();

            // Send the message
            stream.Write(message, 0, message.Length);

            try
            {
                // Retrieve the API response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show("Something went wrong.\n" + response.StatusDescription);
                    return false;
                }
                else
                {
                    MessageBox.Show("Point was successfully inserted into the database.");
                }
            }
            catch (WebException e)
            {
                MessageBox.Show("Something went wrong.\n" + e.Message);
                return false;
            }

            return true;
        }
    }
}
