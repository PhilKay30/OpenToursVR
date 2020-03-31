using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mapping.SvgConverter
{
    public class SvgTabControl : TabControl
    {
        #region Property Management

        /// <summary>
        /// Sets up routing the OnDataLoaded event to the calling class.
        /// </summary>
        public static RoutedEvent OnDataLoadedEvent = EventManager.RegisterRoutedEvent(
            "OnDataLoaded",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SvgTabControl));

        /// <summary>
        /// The event handler for the OnDataLoaded event on the control.
        /// </summary>
        public event RoutedEventHandler OnDataLoaded
        {
            add => AddHandler(OnDataLoadedEvent, value);
            remove => RemoveHandler(OnDataLoadedEvent, value);
        }

        /// <summary>
        /// Sets up routing the OnSelectionChanged event to the calling class.
        /// </summary>
        public static RoutedEvent OnSelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "OnSelectionChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SvgTabControl));

        /// <summary>
        /// The event handler for the OnSelectionChanged event on the control.
        /// </summary>
        public new event RoutedEventHandler OnSelectionChanged
        {
            add => AddHandler(OnSelectionChangedEvent, value);
            remove => RemoveHandler(OnSelectionChangedEvent, value);
        }

        #endregion



        // ReSharper disable AssignNullToNotNullAttribute
        private static Dictionary<string, string> OptionMap { get; } = new Dictionary<string, string>
        {
            {Application.Current.FindResource("TabAerialway")?.ToString(), "aerialway"},
            {Application.Current.FindResource("TabAeroway")?.ToString(), "aeroway"},
            {Application.Current.FindResource("TabAmenity")?.ToString(), "amenity"},
            {Application.Current.FindResource("TabBicycle")?.ToString(), "bicycle"},
            {Application.Current.FindResource("TabBridge")?.ToString(), "bridge"},
            {Application.Current.FindResource("TabBuilding")?.ToString(), "building"},
            {Application.Current.FindResource("TabHarbour")?.ToString(), "harbour"},
            {Application.Current.FindResource("TabHighway")?.ToString(), "highway"},
            {Application.Current.FindResource("TabHistoric")?.ToString(), "historic"},
            {Application.Current.FindResource("TabLanduse")?.ToString(), "landuse"},
            {Application.Current.FindResource("TabLeisure")?.ToString(), "leisure"},
            {Application.Current.FindResource("TabMotorcar")?.ToString(), "motorcar"},
            {Application.Current.FindResource("TabPublicTransport")?.ToString(), "public_transport"},
            {Application.Current.FindResource("TabRailway")?.ToString(), "railway"},
            {Application.Current.FindResource("TabShop")?.ToString(), "shop"},
            {Application.Current.FindResource("TabSport")?.ToString(), "sport"},
            {Application.Current.FindResource("TabTourism")?.ToString(), "tourism"},
            {Application.Current.FindResource("TabWater")?.ToString(), "water"},
            {Application.Current.FindResource("TabWaterway")?.ToString(), "waterway"},
        };

        public static Dictionary<string, List<string>> Options { get; } = new Dictionary<string, List<string>>();

        public bool HasSelection => Options.Count > 0;



        public void LoadDataPoints(List<PostGisData> dataPoints)
        {
            // Iterate through the tabs and add them to the UI
            foreach (TabItem tabItem in GetTabs(dataPoints))
            {
                Items.Add(tabItem);
            }

            RoutedEventArgs args = new RoutedEventArgs(OnDataLoadedEvent, this);
            RaiseEvent(args);
        }

        public void SelectAll()
        {
            // Iterate through the tabs
            foreach (TabItem tabItem in Items)
            {
                // Iterate through the checkboxes in the tab
                ListView listView = (ListView)tabItem.Content;
                foreach (ListViewItem listViewItem in listView.Items)
                {
                    // Deselect the checkbox
                    CheckBox checkBox = (CheckBox)listViewItem.Content;
                    checkBox.IsChecked = true;
                }
            }
        }

        public void DeselectAll()
        {
            // Iterate through the tabs
            foreach (TabItem tabItem in Items)
            {
                // Iterate through the checkboxes in the tab
                ListView listView = (ListView)tabItem.Content;
                foreach (ListViewItem listViewItem in listView.Items)
                {
                    // Deselect the checkbox
                    CheckBox checkBox = (CheckBox)listViewItem.Content;
                    checkBox.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// Retrieves the tabs in the UI.
        /// </summary>
        /// <param name="dataPoints">The data-points from the database</param>
        /// <returns>An enumeration of tabs</returns>
        private IEnumerable<TabItem> GetTabs(List<PostGisData> dataPoints)
        {
            // Iterate through the tab headers
            foreach (string headerName in OptionMap.Keys)
            {
                // Retrieve data key
                if (!OptionMap.TryGetValue(headerName, out string dataKey))
                {
                    continue;
                }

                // Create the tab item
                TabItem tabItem = new TabItem { Header = headerName, Content = new ListView() };

                // Iterate through the data which contains the specified data key
                foreach (IGrouping<string, PostGisData> group in
                    from dataPoint in dataPoints
                    where dataPoint != null
                          && dataPoint.Data.ContainsKey(dataKey)
                          && !string.IsNullOrEmpty(dataPoint.Data[dataKey])
                    group dataPoint by dataPoint.Data[dataKey])
                {
                    // Add the checkbox to the tab
                    AddCheckbox(tabItem, @group.Key);
                }

                // Return the tabs which contain data
                if ((tabItem.Content as ListView)?.Items.Count > 0)
                {
                    yield return tabItem;
                }
            }
        }

        /// <summary>
        /// Adds the specified checkbox to the specified tab.
        /// </summary>
        /// <param name="tabItem">The tab to add the checkbox to</param>
        /// <param name="checkboxName">The name of the checkbox to be added</param>
        private void AddCheckbox(TabItem tabItem, string checkboxName)
        {
            // Ensure that the specified header exists
            if (!OptionMap.TryGetValue(tabItem.Header.ToString(), out string dataKey))
            {
                return;
            }

            List<string> options;

            // Create the checkbox
            CheckBox checkBox = new CheckBox { Content = checkboxName };

            // Add a listener for when the checkbox is checked
            checkBox.Checked += (o, args) =>
            {
                // Check if the tab already has an option list
                if (Options.TryGetValue(dataKey, out options))
                {
                    // Option list exists, so add to it
                    options.Add(checkboxName);
                }
                else
                {
                    // Option list doesn't exist, so create it with the specified option
                    options = new List<string> { checkboxName };
                    Options.Add(dataKey, options);
                }

                OnToggleCheckbox();
            };

            // Add a listener for when the checkbox is unchecked
            checkBox.Unchecked += (o, args) =>
            {
                // Check if tab actually has an option list
                if (!Options.TryGetValue(dataKey, out options))
                {
                    return;
                }

                // Remove the specified option from the option list
                options.Remove(checkboxName);

                // If the option list is empty, remove it from the master list
                if (options.Count == 0)
                {
                    Options.Remove(dataKey);
                }

                OnToggleCheckbox();
            };

            // Add the checkbox to the tab
            (tabItem.Content as ListView)?.Items.Add(new ListViewItem { Content = checkBox, Focusable = false });
        }

        private void OnToggleCheckbox()
        {
            RoutedEventArgs args = new RoutedEventArgs(OnSelectionChangedEvent, this);
            RaiseEvent(args);
        }
    }
}
