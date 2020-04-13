using Microsoft.Win32;
using System;
using System.IO;
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
            (System.Windows.Application.Current.MainWindow as LaunchWindow)?.UpdateNavigation();
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
                "TourPoints/TourPointsPage.xaml", UriKind.Relative));
        }

        private void OnClick_AddModel(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri(
                "Models/ModelsPage.xaml", UriKind.Relative));
        }

        private void OnClick_AddHistMap(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                InitialDirectory = "c:\\",
                Filter = "Select a JPEG|*.jpg",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string inputFilePath = openFileDialog.FileName;
                string outputFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\histMap.jpg";
                File.Copy(inputFilePath, outputFilePath, true);
                LaunchHistMapEditor();
            }
        }

        private static void LaunchHistMapEditor()
        {
            using System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @"../../../../HistMapConfig/UnityTool/HistMapConfigTool.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
