using System;
using System.Windows;
using System.Windows.Controls;

namespace Mapping
{
    public partial class LaunchPage : Page
    {
        public LaunchPage()
        {
            InitializeComponent();
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
    }
}
