using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mapping.SvgConverter
{
    /// <summary>
    /// Helper class to handle generation of an SVG image from specific data-points and options.
    /// Created by Timothy J Cowen.
    /// </summary>
    public static class SvgGenerator
    {
        /// <summary>
        /// A callback to handle interfacing with the UI.
        /// </summary>
        public interface ISvgGenerator
        {
            void OnGenerationFinished(string pathToImage);
        }

        /// <summary>
        /// Generates an SVG image file using the specified data-points, boundaries and options.
        /// Saves the file to the specified location.
        /// </summary>
        /// <param name="outputFile">The location to save the SVG file</param>
        /// <param name="dataPoints">The list of data-points to include</param>
        /// <param name="topLeftPoint">The top-left bound of the data</param>
        /// <param name="bottomRightPoint">The bottom-right bound of the data</param>
        /// <param name="listener">The callback for the UI</param>
        public static async void Generate(
            string outputFile,
            List<PostGisData> dataPoints,
            PostGisPoint topLeftPoint,
            PostGisPoint bottomRightPoint,
            ISvgGenerator listener)
        {
            // Run on background thread
            await Task.Run(() =>
            {
                // Normalize bounds to start at (0,0) and move toward positive (bottom-right) values
                bottomRightPoint.X -= topLeftPoint.X;
                bottomRightPoint.Y -= topLeftPoint.Y;
                topLeftPoint.X -= topLeftPoint.X;
                topLeftPoint.Y -= topLeftPoint.Y;

                // Initialize stream writer
                using StreamWriter sw = new StreamWriter(outputFile);

                // Write the header of the SVG
                sw.Write("<svg viewBox=\"");
                sw.Write(topLeftPoint.X + " " + topLeftPoint.Y + " " + bottomRightPoint.X + " " + bottomRightPoint.Y);
                sw.WriteLine("\" xmlns=\"http://www.w3.org/2000/svg\">");

                // Iterate through the polygons which fit the specified options
                foreach (PostGisData dataPoint in
                    from optionsKey in TabListInterface.Options.Keys
                    let allowedValues = TabListInterface.Options[optionsKey]
                    from dataPoint in
                        from dataPoint in dataPoints
                        where dataPoint.Type == PostGisData.DataType.Polygon
                              && dataPoint.Data.ContainsKey(optionsKey)
                              && allowedValues.Contains(dataPoint.Data[optionsKey])
                        select dataPoint
                    select dataPoint)
                {
                    // Write the polygon path
                    sw.Write("<path d=\"");
                    sw.Write(dataPoint.Data["way"]);
                    sw.Write("\" fill=\"");
                    sw.Write(dataPoint.GetColour());
                    sw.WriteLine("\" stroke=\"none\"/>");
                }

                // Iterate through the rest of the data-points which fit the specified options
                foreach (PostGisData dataPoint in
                    from optionsKey in TabListInterface.Options.Keys
                    let allowedValues = TabListInterface.Options[optionsKey]
                    from dataPoint in
                        from dataPoint in dataPoints
                        where dataPoint.Type != PostGisData.DataType.Polygon
                              && dataPoint.Data.ContainsKey(optionsKey)
                              && allowedValues.Contains(dataPoint.Data[optionsKey])
                        select dataPoint
                    select dataPoint)
                {
                    // Write the road/line/point path
                    sw.Write("<path d=\"");
                    sw.Write(dataPoint.Data["way"]);
                    sw.Write("\" stroke=\"");
                    sw.Write(dataPoint.GetColour());
                    sw.WriteLine("\" stroke-width=\"3\" fill=\"none\"/>");
                }

                // Close the SVG tag
                sw.WriteLine("</svg>");
            }).ContinueWith(task =>
            {
                // Launch the callback
                listener.OnGenerationFinished(outputFile);
            });
        }
    }
}
