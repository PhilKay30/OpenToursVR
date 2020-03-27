using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;
using Mapping.SvgConverter;
using Microsoft.Win32;
using Geopoint = Windows.Devices.Geolocation.Geopoint;
using MapInputEventArgs = Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.MapInputEventArgs;

namespace Mapping.TourPoints
{
    /// <summary>
    /// Interaction logic for the TourPoints page.
    /// Created by Brendan Brading.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class TourPointsPage : Page
    {
        private Geopoint TourPointLocation { get; set; }
        private PostGisPoint BoundTopLeft { get; set; }
        private PostGisPoint BoundBottomRight { get; set; }
        private string ImagePath { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TourPointsPage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoad;
        }

        /// <summary>
        /// Handles updating the forward/back navigation buttons.
        /// Zooms in on selected map, adding box for user to view.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnPageLoad(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as LaunchWindow)?.UpdateNavigation();
            ZoomToSelection();
            AddBoxToMap();
        }


        /// <summary>
        /// Zooms map to current selection.
        /// </summary>
        private void ZoomToSelection()
        {
            // Get dictionary of bounds from API
            IDictionary<string, PostGisPoint> bounds = new ApiHandler.ApiHandler().GetBounds().ToDictionary(pair => pair.Key, pair => pair.Value);

            // Ensure that bounds exist
            if (!bounds.TryGetValue("top_left", out PostGisPoint topLeft)
                || !bounds.TryGetValue("bottom_right", out PostGisPoint botRight))
            {
                // TODO : Add some sort of error
                return;
            }

            BoundTopLeft = topLeft;
            BoundBottomRight = botRight;

            // Calculate the center point
            Geopoint centerGeopoint = new Geopoint(new BasicGeoposition
            {
                Latitude = ((BoundTopLeft.Latitude - BoundBottomRight.Latitude) / 2) + BoundBottomRight.Latitude,
                Longitude = ((BoundBottomRight.Longitude - BoundTopLeft.Longitude) / 2) + BoundTopLeft.Longitude
            });

            // Set viewport to bounds
            MyMapControl.TrySetViewAsync(centerGeopoint, 16);
        }


        /// <summary>
        /// Adds box around the currently selected map.
        /// </summary>
        private void AddBoxToMap()
        {
            List<BasicGeoposition> list = new List<BasicGeoposition>();

            BasicGeoposition topLeft = new BasicGeoposition
            {
                Longitude = BoundTopLeft.Longitude,
                Latitude = BoundTopLeft.Latitude
            };
            list.Add(topLeft);

            BasicGeoposition topRight = new BasicGeoposition
            {
                Longitude = BoundBottomRight.Longitude,
                Latitude = BoundTopLeft.Latitude
            };
            list.Add(topRight);

            BasicGeoposition bottomRight = new BasicGeoposition
            {
                Longitude = BoundBottomRight.Longitude,
                Latitude = BoundBottomRight.Latitude
            };
            list.Add(bottomRight);

            BasicGeoposition bottomLeft = new BasicGeoposition
            {
                Longitude = BoundTopLeft.Longitude,
                Latitude = BoundBottomRight.Latitude
            };
            list.Add(bottomLeft);

            MapPolygon boundaryPolygon = new MapPolygon
            {
                Path = new Geopath(list),
                FillColor = Color.FromArgb(25, 50, 50, 50)
            };

            MyMapControl.MapElements.Add(boundaryPolygon);
        }

        /// <summary>
        /// Displays the currently selected area to the user, as well as any selected point.
        /// </summary>
        private void UpdateSelectionVisual(Geopoint point)
        {
            // Clear any current pins
            MyMapControl.MapElements.Clear();

            // Redraw box around map selection
            AddBoxToMap();

            // Add a pin where the user tapped
            MapIcon pin = new MapIcon { Location = point };
            MyMapControl.MapElements.Add(pin);

            // Keep track of the point
            TourPointLocation = point;
        }

        /// <summary>
        /// Listener for taps (clicks) on the map.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        private void OnTapMap(object sender, MapInputEventArgs args)
        {
            Geopoint tapped = args.Location;
            UpdateSelectionVisual(tapped);
            LabelPoint.Content = tapped.Position.Longitude + " " + tapped.Position.Latitude;
        }

        /// <summary>
        /// Marks an image file for upload.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnClick_AddImage(object sender, RoutedEventArgs e)
        {
            // Initialize the file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            try
            {
                // Attempt to open the file dialog
                if (openFileDialog.ShowDialog() == true)
                {
                    // Get the path of the specified file
                    ImagePath = openFileDialog.FileName;
                    LabelImage.Content = "Image: " + openFileDialog.SafeFileName;
                }
                else
                {
                    // Dialog didn't open correctly
                    LabelImage.Content = "Image: No Image Selected.";
                }
            }
            catch (Exception ex)
            {
                // Something went wrong with the dialog
                LabelImage.Content = "Image: No Image Selected.";
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Saves the point.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnClick_SavePoint(object sender, RoutedEventArgs e)
        {
            // Validate information
            if (string.IsNullOrEmpty(TextBoxName.Text)
                || string.IsNullOrEmpty(TextBoxDescription.Text)
                || TourPointLocation == null)
            {
                MessageBox.Show("A Tour Point must include a location, name, and description");
                return;
            }

            // Create point string for PostGIS
            string point = "POINT(" + TourPointLocation.Position.Longitude + " " + TourPointLocation.Position.Latitude + ")";

            // Use API to save the point
            ApiHandler.ApiHandler handler = new ApiHandler.ApiHandler();
            if (!handler.AddPoint(point, TextBoxName.Text, TextBoxDescription.Text, ImagePath))
            {
                // Something went wrong, so go back
                return;
            }

            // Clear the current information
            MyMapControl.MapElements.Clear();
            AddBoxToMap();
            ImagePath = string.Empty;
            LabelPoint.Content = "Select a Point";
            LabelImage.Content = "Image : No Image Selected";
            TextBoxName.Text = string.Empty;
            TextBoxDescription.Text = string.Empty;
            TourPointLocation = new Geopoint(new BasicGeoposition());
        }
    }
}
