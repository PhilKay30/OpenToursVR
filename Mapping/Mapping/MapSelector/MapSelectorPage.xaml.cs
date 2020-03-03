using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;
using BasicGeoposition = Windows.Devices.Geolocation.BasicGeoposition;
using Geopoint = Windows.Devices.Geolocation.Geopoint;
using MapInputEventArgs = Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.MapInputEventArgs;

namespace Mapping.MapSelector
{
    /// <summary>
    /// Interaction logic for the MapSelector page.
    /// Created by Timothy J Cowen.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MapSelectorPage : Page
    {
        private double mLeft;
        private double mRight;
        private double mTop;
        private double mBottom;
        private readonly OsmPostGisConverter mOsmPostGisConverter = new OsmPostGisConverter();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MapSelectorPage()
        {
            InitializeComponent();
            ResetSelection();
        }

        /// <summary>
        /// Displays the currently selected area to the user.
        /// </summary>
        private void UpdateSelectionVisual()
        {
            // Clear any current polygons/pins
            MyMapControl.MapElements.Clear();

            // Iterate through corner points and add pin icons
            foreach (BasicGeoposition cornerPoint in GetCornerPoints())
            {
                MapIcon pin = new MapIcon { Location = new Geopoint(cornerPoint) };
                MyMapControl.MapElements.Add(pin);
            }

            // Make sure the selection is valid
            if (!IsSelectionValid())
            {
                ButtonQuery.IsEnabled = false;
                return;
            }

            ButtonQuery.IsEnabled = true;

            // Create polygon to display the selection to the user
            MapPolygon polygon = new MapPolygon
            {
                Path = new Geopath(GetCornerPoints()),
                FillColor = Color.FromArgb(50, 50, 50, 50)
            };

            // Add polygon to the map
            MyMapControl.MapElements.Add(polygon);
        }

        /// <summary>
        /// Updates the selection bounds to fit the specified point.
        /// </summary>
        /// <param name="point">The point to fit within the bounds</param>
        private void FitPoint(Geopoint point)
        {
            // Check if selection has been initialized
            if (mLeft >= 300)
            {
                // Selection has not been started, so all dimensions are the same
                mLeft = point.Position.Longitude;
                mRight = point.Position.Longitude;
                mTop = point.Position.Latitude;
                mBottom = point.Position.Latitude;
                return;
            }

            // Update left bound if needed
            if ((mLeft + 180) > (point.Position.Longitude + 180))
            {
                mLeft = point.Position.Longitude;
            }

            // Update right bound if needed
            if ((mRight + 180) < (point.Position.Longitude + 180))
            {
                mRight = point.Position.Longitude;
            }

            // Update top bound if needed
            if ((mTop + 90) < (point.Position.Latitude + 90))
            {
                mTop = point.Position.Latitude;
            }

            // Update bottom bound if needed
            if ((mBottom + 90) > (point.Position.Latitude + 90))
            {
                mBottom = point.Position.Latitude;
            }
        }

        /// <summary>
        /// Resets the current selection bounds.
        /// </summary>
        private void ResetSelection()
        {
            // Reset bounds (outside of valid values)
            mLeft = 600;
            mRight = 600;
            mTop = 600;
            mBottom = 600;

            // Remove polygon from map
            UpdateSelectionVisual();
        }

        /// <summary>
        /// Checks that the current selection is valid.
        /// </summary>
        /// <returns>True if the selection is valid; false otherwise</returns>
        private bool IsSelectionValid()
        {
            return mLeft != mRight
                   && mTop != mBottom
                   && !(mLeft > mRight)
                   && !(mTop < mBottom);
        }

        /// <summary>
        /// Calculates geoposition of corner points.
        /// </summary>
        /// <returns>Iterative list of corner points</returns>
        private IEnumerable<BasicGeoposition> GetCornerPoints()
        {
            // Ensure that at least one point exists
            if (mLeft > 300)
            {
                yield break;
            }

            // Case : Only one point was selected
            if (mLeft == mRight && mTop == mBottom)
            {
                yield return new BasicGeoposition
                {
                    Latitude = mBottom,
                    Longitude = mLeft
                };

                yield break;
            }

            // Case : Only top and bottom are selected
            if (mLeft == mRight)
            {
                yield return new BasicGeoposition
                {
                    Latitude = mBottom,
                    Longitude = mLeft
                };

                yield return new BasicGeoposition
                {
                    Latitude = mTop,
                    Longitude = mLeft
                };

                yield break;
            }

            // Case : Only left and right are selected
            if (mTop == mBottom)
            {
                yield return new BasicGeoposition
                {
                    Latitude = mBottom,
                    Longitude = mLeft
                };

                yield return new BasicGeoposition
                {
                    Latitude = mBottom,
                    Longitude = mRight
                };

                yield break;
            }

            // Case : Four corners exist
            yield return new BasicGeoposition
            {
                Latitude = mBottom,
                Longitude = mLeft
            };
            yield return new BasicGeoposition
            {
                Latitude = mBottom,
                Longitude = mRight
            };
            yield return new BasicGeoposition
            {
                Latitude = mTop,
                Longitude = mRight
            };
            yield return new BasicGeoposition
            {
                Latitude = mTop,
                Longitude = mLeft
            };
        }

        /// <summary>
        /// Listener for taps (clicks) on the map.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Event arguments</param>
        private void OnTapMap(object sender, MapInputEventArgs args)
        {
            Geopoint tapped = args.Location;
            FitPoint(tapped);
            UpdateSelectionVisual();
        }

        /// <summary>
        /// Listener for 'Reset' button clicks.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnClickReset(object sender, RoutedEventArgs e)
        {
            ResetSelection();
        }

        /// <summary>
        /// Listener for 'Query' button clicks.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnClickQuery(object sender, RoutedEventArgs e)
        {
            // Request the map be retrieved
            bool response = OpenStreetMapInterface.GetMap(
                mLeft.ToString(),
                mBottom.ToString(),
                mRight.ToString(),
                mTop.ToString());

            // Check if map retrieval was successful
            if (response)
            {
                MessageBox.Show("Map was retrieved successfully.");

                // Attempt to convert OSM data to PostGis data
                string returnMessage = mOsmPostGisConverter.ConvertOsmToPostGis();

                // Check if conversion worked
                if (returnMessage != null)
                {
                    // Conversion didn't work so display error message
                    MessageBox.Show(returnMessage);
                }
                else
                {
                    // Conversion worked so go back to main menu
                    MessageBox.Show("Mapping selection was successfully uploaded to the database.");
                    ResetSelection();
                    if (NavigationService != null && NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }
                }
            }
            else
            {
                // Map was not retrieved successfully
                MessageBox.Show(
                    "The map was not able to be retrieved. " +
                                "The likely cause for this is that the selected area was too large." +
                                "\nTry selecting a smaller area and querying again.");
            }
        }
    }
}
