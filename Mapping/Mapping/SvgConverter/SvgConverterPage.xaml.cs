using Mapping.Common;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mapping.SvgConverter
{
    /// <summary>
    /// Interaction logic for the SvgConverter page.
    /// Created by Timothy J Cowen.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class SvgConverterPage : Page, SvgGenerator.ISvgGenerator
    {
        private readonly List<PostGisData> mDataPoints = new List<PostGisData>();
        private readonly PostGisPoint mTopLeft = new PostGisPoint();
        private readonly PostGisPoint mBottomRight = new PostGisPoint();
        private readonly DatabaseInterface mDatabase;
        private readonly SynchronizationContext mContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SvgConverterPage()
        {
            // Initialize page
            InitializeComponent();
            this.Loaded += OnPageLoad;

            // Initialize data members
            mContext = SynchronizationContext.Current;
            mDatabase = new DatabaseInterface();

            // Initialize database
            if (mDatabase.Init())
            {
                // Load data from database
                Load();
            }
        }

        /// <summary>
        /// Handles updating the forward/back navigation buttons
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnPageLoad(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as LaunchWindow)?.UpdateNavigation();
        }

        /// <summary>
        /// Reads the data in from the database and stores it locally.
        /// </summary>
        private async void Load()
        {
            // Run on a background thread
            await Task.Run(() =>
            {
                // Calculate the offset points
                mDatabase.GetBounds(mTopLeft, mBottomRight);

                // Load the polygons from the database
                mDataPoints.AddRange(mDatabase.GetPolygons(mTopLeft));

                // Load the roads from the database
                mDataPoints.AddRange(mDatabase.GetRoads(mTopLeft));

                // Load the lines from the database
                mDataPoints.AddRange(mDatabase.GetLines(mTopLeft));

                // Callback on main thread
                RunOnUiThread(o => MyTabControl.LoadDataPoints(mDataPoints));
            });
        }

        /// <summary>
        /// Listener for SVG generation finishing.
        /// </summary>
        /// <param name="pathToImage">The path to the generated image</param>
        public void OnGenerationFinished(string pathToImage)
        {
            // Load the generated image into the UI
            RunOnUiThread(o =>
            {
                DisplayImage.Source = new Uri(pathToImage);
                ButtonSave.IsEnabled = true;
            });
        }

        private void OnTabsLoaded(object sender, RoutedEventArgs e)
        {
            // Enable the buttons
            ButtonSelect.IsEnabled = true;
            ButtonDeselect.IsEnabled = true;
        }

        private void OnOptionSelectionChanged(object sender, RoutedEventArgs e)
        {
            ButtonGenerate.IsEnabled = MyTabControl.HasSelection;
        }

        /// <summary>
        /// Runs the specified callback on the UI thread.
        /// </summary>
        /// <param name="callback">The callback to run</param>
        private void RunOnUiThread(SendOrPostCallback callback)
        {
            // Run the specified code against the main context
            mContext.Post(callback, null);
        }

        /// <summary>
        /// Selects all potential options for generation.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnClick_SelectAll(object sender, RoutedEventArgs e)
        {
            MyTabControl.SelectAll();
        }

        /// <summary>
        /// Deselects all potential options for generation.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnClick_DeselectAll(object sender, RoutedEventArgs e)
        {
            MyTabControl.DeselectAll();
        }

        /// <summary>
        /// Begins image generation.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnClick_GenerateImage(object sender, RoutedEventArgs e)
        {
            // Make sure that something has been selected to display
            if (!MyTabControl.HasSelection)
            {
                DisplayImage.Source = null;
                ButtonSave.IsEnabled = false;
                MessageBox.Show(Application.Current.FindResource("PromptImageGenerationMustSelect")?.ToString());
            }
            else
            {
                // Request a generated SVG
                SvgGenerator.Generate(
                    FileIO.GetOutputDirectory() + "\\output.svg",
                    mDataPoints,
                    mTopLeft,
                    mBottomRight,
                    this);
            }
        }

        /// <summary>
        /// Begins image saving to database.
        /// Created by Brendan Brading, Fred Chappuis, and Phillip Kempton.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnClick_SaveImage(object sender, RoutedEventArgs e)
        {
            ApiHandler.ApiHandler handler = new ApiHandler.ApiHandler();

            // Call DB to get the points. 
            List<KeyValuePair<string, PostGisPoint>> points = handler.GetBounds();
            PostGisPoint topLeft = new PostGisPoint();
            PostGisPoint botRight = new PostGisPoint();

            foreach ((string key, PostGisPoint value) in points)
            {
                switch (key)
                {
                    case "top_left":
                        topLeft = value;
                        break;
                    case "bottom_right":
                        botRight = value;
                        break;
                    default:
                        throw new Exception("Too Many Bounds");
                }
            }

            // Get the width and height of the map in kilometers using the haversine conversion
            double widthInKm = HaversineConversion.HaversineDistance(
                topLeft,
                new PostGisPoint { X = botRight.Longitude, Y = topLeft.Latitude },
                HaversineConversion.DistanceUnit.Kilometers);
            double heightInKm = HaversineConversion.HaversineDistance(
                topLeft,
                new PostGisPoint { X = topLeft.Longitude, Y = botRight.Latitude },
                HaversineConversion.DistanceUnit.Kilometers);

            int width = (int)Math.Ceiling(5000 * widthInKm);
            int height = (int)Math.Ceiling(5000 * heightInKm);

            // Create a new bitmap with the required size
            Bitmap bitmap = new Bitmap(
                width,
                height,
                PixelFormat.Format32bppArgb);

            // Fill the bitmap background with the specified colour
            using (Graphics bitmapGraphics = Graphics.FromImage(bitmap))
            {
                bitmapGraphics.FillRegion(
                    Brushes.White,
                    new Region(new Rectangle(0, 0, bitmap.Width, bitmap.Height)));
            }

            // Load the generated SVG
            SvgDocument svgDocument = SvgDocument.Open(FileIO.GetOutputDirectory() + @"\output.svg");

            // Draw the contents of the SVG onto the bitmap
            svgDocument.Draw(bitmap);

            // Save the bitmap as a PNG
            bitmap.Save(FileIO.GetOutputDirectory() + @"\output.png", ImageFormat.Png);

            // Push image to API
            handler.InsertPng(
                0.0f,
                @"\output.png",
                "osmMap.png",
                new PostGisPoint { X = (((botRight.Longitude - topLeft.Longitude) / 2) + topLeft.Longitude), Y = (((topLeft.Latitude - botRight.Latitude) / 2) + botRight.Latitude) },
                widthInKm,
                heightInKm);
        }
    }
}

