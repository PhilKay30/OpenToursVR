using Mapping.Common;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Svg;

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
        /// Reads the data in from the database and stores it locally.
        /// </summary>
        private async void Load()
        {
            // Run on a background thread
            await Task.Run(() =>
            {
                // Calculate the offset points
                RunOnUiThread(o => StatusBlock.Text = "Calculating offset...");
                mDatabase.GetBounds(mTopLeft, mBottomRight);

                // Load the polygons from the database
                RunOnUiThread(o => StatusBlock.Text = "Loading polygons...");
                mDataPoints.AddRange(mDatabase.GetPolygons(mTopLeft));

                // Load the roads from the database
                RunOnUiThread(o => StatusBlock.Text = "Loading roads...");
                mDataPoints.AddRange(mDatabase.GetRoads(mTopLeft));

                // Load the lines from the database
                RunOnUiThread(o => StatusBlock.Text = "Loading lines...");
                mDataPoints.AddRange(mDatabase.GetLines(mTopLeft));

                // Callback on main thread
                RunOnUiThread(o => OnLoadFinished());
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

        /// <summary>
        /// Listener for database data load finishing.
        /// </summary>
        private void OnLoadFinished()
        {
            StatusBlock.Text = "Generating options...";

            // Iterate through the tabs and add them to the UI
            foreach (TabItem tabItem in TabListInterface.GetTabs(mDataPoints))
            {
                TabRoads.Items.Add(tabItem);
            }

            // Enable the buttons
            StatusBlock.Text = "Ready";
            ButtonSelect.IsEnabled = true;
            ButtonDeselect.IsEnabled = true;
            ButtonGenerate.IsEnabled = true;
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
            // Iterate through the tabs
            foreach (TabItem tabItem in TabRoads.Items)
            {
                // Iterate through the checkboxes in the tab
                ListView listView = (ListView) tabItem.Content;
                foreach (ListViewItem listViewItem in listView.Items)
                {
                    // Deselect the checkbox
                    CheckBox checkBox = (CheckBox) listViewItem.Content;
                    checkBox.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// Deselects all potential options for generation.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnClick_DeselectAll(object sender, RoutedEventArgs e)
        {
            // Iterate through the tabs
            foreach (TabItem tabItem in TabRoads.Items)
            {
                // Iterate through the checkboxes in the tab
                ListView listView = (ListView) tabItem.Content;
                foreach (ListViewItem listViewItem in listView.Items)
                {
                    // Deselect the checkbox
                    CheckBox checkBox = (CheckBox) listViewItem.Content;
                    checkBox.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// Begins image generation.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnClick_GenerateImage(object sender, RoutedEventArgs e)
        {
            // Make sure that something has been selected to display
            if (TabListInterface.Options.Count == 0)
            {
                DisplayImage.Source = null;
                ButtonSave.IsEnabled = false;
                MessageBox.Show("You must select something to display.");
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
        /// Begins image saving to database. This method was mobbed by Brendan Brading, Fred Chappuis and Phillip Kempton
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnClick_SaveImage(object sender, RoutedEventArgs e)
        {
            
            ApiHandler.ApiHandler handler = new ApiHandler.ApiHandler();
                // call DB to get the points. 
                List<KeyValuePair<string, PostGisPoint>> points = handler.GetBounds();
                PostGisPoint topLeft = new PostGisPoint();
                PostGisPoint botRight = new PostGisPoint();

                foreach (KeyValuePair<string, PostGisPoint> pair in points)
                {
                     
                    if (pair.Key == "top_left")
                    {
                        topLeft = pair.Value;
                    }
                    else if (pair.Key == "bottom_right")
                    {
                        botRight = pair.Value;
                    }
                    else
                    {
                        throw new Exception("Too Many Bounds");
                    }
                }

            // Get the width and height of the map in kilometers using the haversine conversion
                double km_height = HaversineConversion.HaversineDistance(topLeft,
                    new PostGisPoint() {X = topLeft.Longitude, Y = botRight.Latitude},
                    HaversineConversion.DistanceUnit.Kilometers);
                double km_width = HaversineConversion.HaversineDistance(topLeft,
                    new PostGisPoint() {X = botRight.Longitude, Y = topLeft.Latitude},
                    HaversineConversion.DistanceUnit.Kilometers);
                int px_width = (int) Math.Ceiling(5000 * km_width);
                int px_height = (int) Math.Ceiling(5000 * km_height);

                // Generate the SVG into a PNG 
                var svgDocument = SvgDocument.Open(FileIO.GetOutputDirectory() + @"\output.svg");
                using (var smallBitmap = svgDocument.Draw())
                {
                    var width = px_width;
                    var height = px_height;

                    using (var bitmap = svgDocument.Draw(width, height)) //I render again
                    {
                        bitmap.Save(FileIO.GetOutputDirectory() + @"\output.png", ImageFormat.Png);
                    }
                }

                // Push image to API

                handler.InsertPNG(0.0f, @"\output.png", "osmMap.png",
                    new PostGisPoint() {X = topLeft.Longitude, Y = botRight.Latitude});
            }
        }
    }

