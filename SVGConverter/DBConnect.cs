using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace SVGConverter
{
    class DbConnect
    {
        private const string CONNECTION_STRING =
            "Host=192.0.203.84;Port=5432;Username=doctor;Database=capstone;Password=wh0";

        private const string QUERY_STRING =
            "SELECT access, \"addr:housename\", \"addr:housenumber\", \"addr:interpolation\", admin_level, aerialway," +
            " aeroway, amenity, area, barrier, bicycle, brand, bridge, boundary, building, construction," +
            " covered, culvert, cutting, denomination, disused, embankment, foot, \"generator:source\", harbour," +
            " highway, historic, horse, intermittent, junction, landuse, layer, leisure, lock, man_made," +
            " military, motorcar, name, \"natural\", office, oneway, operator, place, population, power," +
            " power_source, public_transport, railway, ref, religion, route, service, shop, sport, surface," +
            " toll, tourism, \"tower:type\", tracktype, tunnel, water, waterway, wetland, width, wood, z_order," +
            " way_area, ST_AsSVG(way,1) AS way FROM ";

        private readonly NpgsqlConnection mConnection;

        public DbConnect()
        {
            mConnection = new NpgsqlConnection(CONNECTION_STRING);
            mConnection.Open();
        }

        public MyPoint GetCenterPoint(string source)
        {
            MyPoint center = new MyPoint();
            using NpgsqlCommand cmd = new NpgsqlCommand("SELECT ST_AsText(ST_Centroid(ST_Extent(way))) FROM " + source + ";", mConnection);
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string data = reader.GetString(0);
                data = data?.Trim();

                if (data == null || !data.Contains("POINT("))
                {
                    continue;
                }

                data = data[(data.IndexOf('(') + 1)..data.IndexOf(')')];
                string[] dataPoints = data.Split(' ');
                center.First = double.Parse(dataPoints[0]);
                center.Second = double.Parse(dataPoints[1]);
            }

            return center;
        }

        private IEnumerable<Data> GetData(string query, MyPoint centerPoint = null)
        {
            using NpgsqlCommand cmd = new NpgsqlCommand(query, mConnection);
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Data data = new Data(reader);

                data.Way = data.Way?.Trim();
                if (data.Way == null || !data.Way.StartsWith('M'))
                {
                    continue;
                }
                data.Way = FixThings(data.Way, centerPoint);

                yield return data;
            }
        }


        public IEnumerable<Data> GetPolygons(MyPoint centerPoint = null)
        {
            string query = "SELECT aerialway, aeroway, amenity, landuse, leisure, sport, tourism, water," +
                           " waterway, ST_AsSVG(way,1) AS way FROM planet_osm_polygon;";

            return GetData(query, centerPoint);
        }

        public IEnumerable<Data> GetRoads(MyPoint centerPoint = null)
        {
            string query = "SELECT bicycle, bridge, highway, public_transport, railway," +
                           " ST_AsSVG(way,1) AS way FROM planet_osm_roads;";

            return GetData(query, centerPoint);
        }

        public IEnumerable<Data> GetLines(MyPoint centerPoint = null)
        {
            string query = "SELECT bicycle, bridge, highway, public_transport, railway," +
                           " ST_AsSVG(way,1) AS way FROM planet_osm_line WHERE highway <> '';";

            return GetData(query, centerPoint);
        }

        public IEnumerable<Data> GetPoints(MyPoint centerPoint = null)
        {
            string query = "SELECT amenity, shop, tourism, ST_AsSVG(way,1) AS way FROM planet_osm_point;";

            return GetData(query, centerPoint);
        }

        private string FixThings(string thing, MyPoint centerPoint)
        {
            if (centerPoint == null)
            {
                return thing;
            }

            string[] things = thing.Split(' ');
            int i = 0;

            while (!double.TryParse(things[i], out _))
            {
                i++;
            }

            double one = double.Parse(things[i]);
            double two = double.Parse(things[i + 1]);

            one -= centerPoint.First;
            two += centerPoint.Second;

            things[i] = one.ToString();
            things[i + 1] = two.ToString();

            StringBuilder builder = new StringBuilder();

            foreach (string thingTemp in things)
            {
                builder.Append(thingTemp).Append(' ');
            }

            return builder.ToString();
        }
    }
}
