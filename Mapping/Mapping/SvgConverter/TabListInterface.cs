using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Mapping.SvgConverter
{
    /// <summary>
    /// Helper class to handle interactions with the tabs in the UI.
    /// Created by Timothy J Cowen.
    /// </summary>
    internal static class TabListInterface
    {
        private static Dictionary<string, string> OptionMap { get; } = new Dictionary<string, string>()
        {
            {"Aerialway", "aerialway"},
            {"Aeroway", "aeroway"},
            {"Amenity", "amenity"},
            {"Bicycle", "bicycle"},
            {"Bridge", "bridge"},
            {"Building", "building"},
            {"Harbour", "harbour"},
            {"Highway", "highway"},
            {"Historic", "historic"},
            {"Landuse", "landuse"},
            {"Leisure", "leisure"},
            {"Motorcar", "motorcar"},
            {"PublicTransport", "public_transport"},
            {"Railway", "railway"},
            {"Shop", "shop"},
            {"Sport", "sport"},
            {"Tourism", "tourism"},
            {"Water", "water"},
            {"Waterway", "waterway"},
        };
        public static Dictionary<string, List<string>> Options { get; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// Retrieves the tabs in the UI.
        /// </summary>
        /// <param name="dataPoints">The data-points from the database</param>
        /// <returns>An enumeration of tabs</returns>
        public static IEnumerable<TabItem> GetTabs(List<PostGisData> dataPoints)
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
                    AddCheckbox(tabItem, group.Key);
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
        private static void AddCheckbox(TabItem tabItem, string checkboxName)
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
            };

            // Add the checkbox to the tab
            (tabItem.Content as ListView)?.Items.Add(new ListViewItem { Content = checkBox, Focusable = false });
        }
    }
}
