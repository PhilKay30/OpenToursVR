using Mapping.SvgConverter;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;
using BasicGeoposition = Windows.Devices.Geolocation.BasicGeoposition;
using Geopoint = Windows.Devices.Geolocation.Geopoint;
using MapInputEventArgs = Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.MapInputEventArgs;

namespace Mapping.Models
{
    /// <summary>
    /// Interaction logic for the Models page.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class ModelsPage : Page
    {
        private Geopoint ModelLocation { get; set; }
        private PostGisPoint BoundTopLeft { get; set; }
        private PostGisPoint BoundBottomRight { get; set; }
        private string ModelPath { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelsPage()
        {
            InitializeComponent();
            Loaded += OnPageLoad;
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
            UpdateSelectionVisual();
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
        private void UpdateSelectionVisual(Geopoint point = null)
        {
            // Clear any current pins
            MyMapControl.MapElements.Clear();

            // Redraw box around map selection
            AddBoxToMap();

            // Check if point is actually specified
            if (point == null)
            {
                // Point was not specified, so clear everything
                ModelPath = string.Empty;
                LabelPoint.Content = Application.Current.FindResource("LabelNoPointSelected");
                LabelModel.Content = Application.Current.FindResource("LabelNoModelSelected");
                ButtonSelectModel.IsEnabled = false;
            }
            else
            {
                // Point was specified, so add it to the map and enable fields
                MapIcon pin = new MapIcon { Location = point };
                MyMapControl.MapElements.Add(pin);
                ButtonSelectModel.IsEnabled = true;
            }

            // Keep track of the point
            ModelLocation = point;
        }

        /// <summary>
        /// Launches Unity tool.
        /// </summary>
        private static void LaunchUnityTool()
        {
            using Process process = new Process
            {
                StartInfo =
                {
                    FileName = @"../../../../Models/UnityTool/ModelConfigTool.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }

        /// <summary>
        /// Creates the required directories if they do not already exist.
        /// </summary>
        private static void CreateDirectory()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OpenToursVR"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OpenToursVR");
            }

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OpenToursVR\\Models"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OpenToursVR\\Models");
            }
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
        /// Marks a model file for upload.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnClick_AddModel(object sender, RoutedEventArgs e)
        {
            // Initialize the file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "OBJ Model Files|*.obj",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            try
            {
                // Attempt to open the file dialog
                if (openFileDialog.ShowDialog() == true)
                {
                    // Get the path of the specified file
                    ModelPath = openFileDialog.FileName;
                    LabelModel.Content = openFileDialog.SafeFileName;
                }
                else
                {
                    // Dialog didn't open correctly
                    LabelModel.Content = Application.Current.FindResource("LabelNoModelSelected");
                }
            }
            catch (Exception ex)
            {
                // Something went wrong with the dialog
                LabelModel.Content = Application.Current.FindResource("LabelNoModelSelected");
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Saves the point.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnClick_SaveModel(object sender, RoutedEventArgs e)
        {
            // Validate information
            if (ModelLocation == null || ModelPath == null)
            {
                MessageBox.Show(Application.Current.FindResource("PromptModelMustInclude")?.ToString());
                return;
            }

            string[] lines = new string[3];
            lines[0] = ModelPath + "\n";
            lines[1] = ModelLocation.Position.Longitude + "\n";
            lines[2] = ModelLocation.Position.Latitude.ToString();
            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\OpenToursVR\\Models\\ModelData.txt";

            CreateDirectory();

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            // Create a new file     
            using (FileStream fs = File.Create(outputPath))
            {
                foreach (string lineString in lines)
                {
                    byte[] line = new UTF8Encoding(true).GetBytes(lineString);
                    fs.Write(line, 0, line.Length);
                }
            }
            // Launch the Unity app
            LaunchUnityTool();

            // Clear the current information
            UpdateSelectionVisual();
        }

        /// <summary>
        /// Resets the point and fields.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnClick_ResetModel(object sender, RoutedEventArgs e)
        {
            UpdateSelectionVisual();
        }
    }
}
