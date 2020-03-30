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
using System.IO;
using System.Text;

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
            ModelLocation = point;
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
                        foreach (string s in lines)
                        {
                            Byte[] line = new UTF8Encoding(true).GetBytes(s);
                            fs.Write(line, 0, line.Length);
                        }
                    }
                    // Launch the Unity app
                    LaunchUnityTool();
                }
                else
                {
                    // Dialog didn't open correctly
                    ClearInfo();
                }
            }
            catch (Exception ex)
            {
                // Something went wrong with the dialog
                ClearInfo();
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex.Message);
            }
        }

        private void LaunchUnityTool()
        {
            using System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @"../../../../Models/UnityTool/New Unity Project.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private void CreateDirectory()
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
        /// Clears any previosly selected / entered info or files
        /// </summary>
        private void ClearInfo()
        {
            // Clear the current information
            MyMapControl.MapElements.Clear();
            AddBoxToMap();
            ModelPath = string.Empty;
            LabelPoint.Content = "No Point Selected";
            ModelLocation = new Geopoint(new BasicGeoposition());
        }
    }
}
