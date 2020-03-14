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

namespace Mapping.DataPoints
{
    /// <summary>
    /// Interaction logic for the MapSelector page.
    /// Created by Timothy J Cowen.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class DataPoints : Page
    {
        private Geopoint PointGeopoint { get; set; }
        private PostGisPoint TopLeft { get; set; }
        private PostGisPoint BottomRight { get; set; }
        private string pngPath { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataPoints()
        {
            InitializeComponent();
            this.Loaded += OnPageLoad;
        }

        /// <summary>
        /// Handles updating the forward/back navigation buttons and zooms in on selected map, adding box for user to view
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
        /// Zooms map to current selection
        /// </summary>
        private void ZoomToSelection()
        {
            // get dict of bounds from api, pull values out into properties
            IDictionary<string, PostGisPoint> dic = new ApiHandler.ApiHandler().GetBounds().ToDictionary(pair => pair.Key, pair => pair.Value);
            dic.TryGetValue("top_left", out var topLeft);
            TopLeft = topLeft;
            dic.TryGetValue("bottom_right", out var botRight);
            BottomRight = botRight;

            // calculate the center point and center the map there with STATIC zoom
            BasicGeoposition center = new BasicGeoposition();
            center.Latitude = ((TopLeft.Latitude - BottomRight.Latitude) / 2) + BottomRight.Latitude;
            center.Longitude = ((BottomRight.Longitude - TopLeft.Longitude) / 2) + TopLeft.Longitude;
            Geopoint centerGeopoint = new Geopoint(center);
            DataSelector.TrySetViewAsync(centerGeopoint, 16);
        }


        /// <summary>
        /// Adds box around the currently selected map
        /// </summary>
        private void AddBoxToMap()
        {
            List<BasicGeoposition> list = new List<BasicGeoposition>();

            BasicGeoposition TL = new BasicGeoposition()
            {
                Longitude = TopLeft.Longitude,
                Latitude = TopLeft.Latitude
            };
            list.Add(TL);
            BasicGeoposition TR = new BasicGeoposition()
            {
                Longitude = BottomRight.Longitude,
                Latitude = TopLeft.Latitude
            };
            list.Add(TR);
            BasicGeoposition BR = new BasicGeoposition()
            {
                Longitude = BottomRight.Longitude,
                Latitude = BottomRight.Latitude
            };
            list.Add(BR);
            BasicGeoposition BL = new BasicGeoposition()
            {
                Longitude = TopLeft.Longitude,
                Latitude = BottomRight.Latitude
            };
            list.Add(BL);
            MapPolygon poly = new MapPolygon()
            {
                Path = new Geopath(list),
                FillColor = Color.FromArgb(25, 50, 50, 50)
            };

            DataSelector.MapElements.Add(poly);
        }

        /// <summary>
        /// Displays the currently selected area to the user, as well as any selected point
        /// </summary>
        private void UpdateSelectionVisual(Geopoint point)
        {
            // Clear any current pins
            DataSelector.MapElements.Clear();

            // redraw bos around map selection
            AddBoxToMap();

            // Iterate through corner points and add pin icons
            MapIcon pin = new MapIcon { Location = point };
            DataSelector.MapElements.Add(pin);
            
            // This is for the point for the api call.
            PointGeopoint = point;
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
            LblPoint.Content = tapped.Position.Longitude.ToString() + " " + tapped.Position.Latitude.ToString();
        }

        /// <summary>
        /// This will utilize the open file dialog, and upload the png as a byte array
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadPng_Click(object sender, RoutedEventArgs e)
        {
            // The following is using the Open File Dialog from Microsoft Docs
            OpenFileDialog openFileDialog = new OpenFileDialog();
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;
                try
                {
                    if (openFileDialog.ShowDialog() == true)
                    {
                        //Get the path of specified file
                        pngPath = openFileDialog.FileName;

                        LblImage.Content ="Image: " + openFileDialog.SafeFileName;
                    }
                    else
                    {
                        LblImage.Content = "Image: No Image Selected.";
                    }
                }
                catch (Exception ee)
                {
                    LblImage.Content = "Image: No Image Selected.";
                    Debug.WriteLine(ee.Message);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavePoint_Click(object sender, RoutedEventArgs e)
        {
            // Validate information, call API
            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrEmpty(TxtDesc.Text) ||
                PointGeopoint == null)
            {
                MessageBox.Show("A Data Point must include a Location, a Name, and a Description");
                return;
            }
            string name = TxtName.Text;
            string point = "POINT(" + PointGeopoint.Position.Longitude.ToString() + " " + PointGeopoint.Position.Latitude.ToString() + ")";
            string desc = TxtDesc.Text;
            
            ApiHandler.ApiHandler handler = new ApiHandler.ApiHandler();
            if (!handler.AddPoint(point, name, desc, pngPath)) 
                return;
            
            
            // If successful, clear screen
            DataSelector.MapElements.Clear();
            AddBoxToMap();
            pngPath = string.Empty;
            LblImage.Content = "Image : No Image Selected";
            LblPoint.Content = "Select a Point";

            TxtName.Text = string.Empty;
            TxtDesc.Text = string.Empty;

            PointGeopoint = new Geopoint(new BasicGeoposition());
        }
    }
}
