using System.Windows;
using System.Windows.Controls;

namespace Mapping.HistoricalMap
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class HistoricalMapPage : Page
    {
        public HistoricalMapPage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoad;
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
    }
}
