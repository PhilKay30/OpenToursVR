using System;
using Npgsql;

namespace SVGConverter
{
    class Data
    {
        public string Aerialway { get; set; }
        public string Aeroway { get; set; }
        public string Amenity { get; set; }
        public string Bicycle { get; set; }
        public string Bridge { get; set; }
        public string Building { get; set; }
        public string Harbour { get; set; }
        public string Highway { get; set; }
        public string Historic { get; set; }
        public string Landuse { get; set; }
        public string Leisure { get; set; }
        public string Motorcar { get; set; }
        public string PublicTransport { get; set; }
        public string Railway { get; set; }
        public string Shop { get; set; }
        public string Sport { get; set; }
        public string Tourism { get; set; }
        public string Water { get; set; }
        public string Waterway { get; set; }
        public string Way { get; set; }

        public Data(NpgsqlDataReader reader)
        {
            Aerialway = GetValueFromReader(reader, "aerialway");
            Aeroway = GetValueFromReader(reader, "aeroway");
            Amenity = GetValueFromReader(reader, "amenity");
            Bicycle = GetValueFromReader(reader, "bicycle");
            Bridge = GetValueFromReader(reader, "bridge");
            Building = GetValueFromReader(reader, "building");
            Harbour = GetValueFromReader(reader, "harbour");
            Highway = GetValueFromReader(reader, "highway");
            Historic = GetValueFromReader(reader, "historic");
            Landuse = GetValueFromReader(reader, "landuse");
            Leisure = GetValueFromReader(reader, "leisure");
            Motorcar = GetValueFromReader(reader, "motorcar");
            PublicTransport = GetValueFromReader(reader, "public_transport");
            Railway = GetValueFromReader(reader, "railway");
            Shop = GetValueFromReader(reader, "shop");
            Sport = GetValueFromReader(reader, "sport");
            Tourism = GetValueFromReader(reader, "tourism");
            Water = GetValueFromReader(reader, "water");
            Waterway = GetValueFromReader(reader, "waterway");
            Way = GetValueFromReader(reader, "way");
        }

        public string GetColour()
        {
            if (!string.IsNullOrWhiteSpace(Amenity))
            {
                return "grey";
            }

            if (!string.IsNullOrWhiteSpace(Building))
            {
                return "grey";
            }

            if (!string.IsNullOrWhiteSpace(Waterway))
            {
                return "blue";
            }

            if (!string.IsNullOrWhiteSpace(Railway))
            {
                return "black";
            }

            if (!string.IsNullOrWhiteSpace(Highway))
            {
                return "red";
            }

            if (!string.IsNullOrWhiteSpace(Bicycle) || !string.IsNullOrWhiteSpace(PublicTransport))
            {
                return "yellow";
            }

            return "green";
        }

        private string GetValueFromReader(NpgsqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.GetString(ordinal);
            }
            catch (InvalidCastException)
            {
                return "";
            }
            catch (IndexOutOfRangeException)
            {
                return "";
            }
        }
    }
}
