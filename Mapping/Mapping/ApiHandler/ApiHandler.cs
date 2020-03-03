using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Windows;
using Mapping.Common;
using Mapping.SvgConverter;
using Npgsql;

namespace Mapping.ApiHandler
{
    class ApiHandler
    {
        /// <summary>
        /// Inserts the PNG into the Database
        /// </summary>
        /// <param name="rotation">The rotation of the image</param>
        /// <param name="imgpath">Path of where the image is stored.</param>
        /// <param name="name">Name of the image</param>
        /// <param name="botLeftCorner">The GIS data of the bottom left corner</param>
        public void InsertPNG(double rotation, string imgpath, string name, PostGisPoint botLeftCorner)
        {
            // Get the bytes from PNG file and convert to hex string so Python doesn't shit itself
            byte[] ba = File.ReadAllBytes(FileIO.GetOutputDirectory()+imgpath);
            string myString = String.Concat(ba.Select(b => b.ToString("X2")).ToArray());

            // create object to serialize into JSON and then serialize it
            MyObj obj = new MyObj();
            obj.img_name = name;
            obj.img = myString;
            obj.img_size = myString.Length;
            obj.corner = String.Format("POINT({0} {1})", botLeftCorner.Longitude, botLeftCorner.Latitude);
            obj.img_rotation = rotation;

            // Serialize for the message, duh
            string jsonString = JsonSerializer.Serialize(obj);

            // create the stream to the API and make the call
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(@"http://192.0.203.84:5000/db_api/addimg");
            req.ContentType = "application/json; charset=utf-8";
            req.Method = "POST";
            byte[] msg = new ASCIIEncoding().GetBytes(jsonString);
            req.ContentLength = msg.Length;
            Stream newStream = req.GetRequestStream();
            newStream.Write(msg, 0, msg.Length); // Send the data.

            string text;
            var response = (HttpWebResponse)req.GetResponse();
            MessageBox.Show("inserted png in database.");

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }
        }

        /// <summary>
        /// TODO: Set this to an API call
        /// </summary>
        /// <returns>List of KvP of the pairs</returns>
        public List<KeyValuePair<string, PostGisPoint>> GetBounds()
        {
            string qry = "SELECT name, ST_AsText(geom) AS point FROM bounds;";
            List<KeyValuePair<string, PostGisPoint>> points = new List<KeyValuePair<string, PostGisPoint>>();

            using (NpgsqlConnection conn = new NpgsqlConnection("Host=192.0.203.84;" +
                                                                "Port=5432;" +
                                                                "Database=capstone;" +
                                                                "Username=doctor;" +
                                                                "Password=wh0;" +
                                                                "Timeout=8;" +
                                                                "Command Timeout=8"))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(qry, conn))
                {
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
                                PostGisPoint topLeftPoint = new PostGisPoint();
                                // Parse top left point
                                topLeftPoint.X = double.Parse(point[(point.IndexOf("(") + 1)..point.IndexOf(" ")]);
                                topLeftPoint.Y = double.Parse(point[(point.IndexOf(" ") + 1)..point.IndexOf(")")]);
                                points.Add(new KeyValuePair<string, PostGisPoint>("top_left", topLeftPoint));
                            }
                            // bottom_right corner
                            else if (name.Equals("bottom_right"))
                            {
                                PostGisPoint bottomRightPoint = new PostGisPoint(); 
                                // Parse bottom right point
                                bottomRightPoint.X = double.Parse(point[(point.IndexOf("(") + 1)..point.IndexOf(" ")]);
                                bottomRightPoint.Y = double.Parse(point[(point.IndexOf(" ") + 1)..point.IndexOf(")")]);
                                points.Add(new KeyValuePair<string, PostGisPoint>("bottom_right", bottomRightPoint));
                            }
                            else
                            {
                                // YEP
                                throw new Exception("Database is fucked");
                            }
                        }
                    }
                    // Shit went drastically wrong
                    catch (Exception)
                    {
                        MessageBox.Show("There are more than 4 sides to the map.\n" +
                                        "Clearing Data, please start over.");
                    }
                }
            }
            return points;
        }

        /// <summary>
        /// Generic class for the JSON object.
        /// </summary>
        private class MyObj
        {
            public string img_name { get; set; }
            public string img { get; set; }
            public int img_size { get; set; }
            public string corner { get; set; }
            public double img_rotation { get; set; }
        }
    }
}
