using System;
using System.Windows;
using System.Windows.Controls;

namespace Mapping
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once RedundantExtendsListEntry
    public partial class LaunchPage : Page
    {
        public LaunchPage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoad;
        }

        /// <summary>
        /// Handles updating the forward/back navigation buttons.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void OnPageLoad(object sender, RoutedEventArgs e)
        {
            // Update navigation buttons
            (Application.Current.MainWindow as LaunchWindow)?.UpdateNavigation();
        }

        private void OnClick_SelectLocation(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri(
                "MapSelector/MapSelectorPage.xaml", UriKind.Relative));
        }

        private void OnClick_GenerateImage(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri(
                "SvgConverter/SvgConverterPage.xaml", UriKind.Relative));
        }

        private void OnClick_AddTourPoint(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri(
                "DataPoints/DataPoints.xaml", UriKind.Relative));
        }
    }
}
