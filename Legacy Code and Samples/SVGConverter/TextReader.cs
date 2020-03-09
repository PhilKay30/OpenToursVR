using System;
using System.IO;

namespace SVGConverter
{
    class TextReader
    {
        public TextReader(string outputFile)
        {
            DbConnect connect = new DbConnect();
            MyPoint centerPoint = connect.GetCenterPoint("planet_osm_roads");

            using StreamWriter sw = new StreamWriter(outputFile);

            sw.WriteLine("<svg xmlns=\"http://www.w3.org/2000/svg\">");

            Console.WriteLine("Generating polygons...");
            foreach (Data polygon in connect.GetPolygons(centerPoint))
            {
                sw.WriteLine("<path d=\"" + polygon.Way + "\" stroke=\"none\" fill=\"" + polygon.GetColour() + "\"/>");
            }

            Console.WriteLine("Generating Roads...");
            foreach (Data polygon in connect.GetRoads(centerPoint))
            {
                sw.WriteLine("<path d=\"" + polygon.Way + "\" stroke=\"" + polygon.GetColour() + "\" fill=\"none\"/>");
            }

            Console.WriteLine("Generating Lines...");
            foreach (Data polygon in connect.GetLines(centerPoint))
            {
                sw.WriteLine("<path d=\"" + polygon.Way + "\" stroke=\"" + polygon.GetColour() + "\" fill=\"none\"/>");
            }

            // Console.WriteLine("Generating Points...");
            // foreach (Data polygon in connect.GetPoints(centerPoint))
            // {
            //     sw.WriteLine("<path d=\"" + polygon.Way + "\" stroke=\"red\" fill=\"none\"/>");
            // }

            sw.WriteLine("</svg>");
        }
    }
}
