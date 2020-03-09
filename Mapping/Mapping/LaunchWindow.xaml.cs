﻿using Mapping.MapSelector;
using Mapping.SvgConverter;
using System.Windows;
using System.Windows.Navigation;

namespace Mapping
{
    public partial class LaunchWindow : Window
    {
        private readonly NavigationService mNavigationService;

        public LaunchWindow()
        {
            InitializeComponent();
            mNavigationService = ContentFrame.NavigationService;
            UpdateNavigation();
        }

        private void OnClick_Back(object sender, RoutedEventArgs e)
        {
            if (mNavigationService != null && mNavigationService.CanGoBack)
            {
                mNavigationService.GoBack();
            }

            UpdateNavigation();
        }

        private void OnClick_Forward(object sender, RoutedEventArgs e)
        {
            if (mNavigationService != null && mNavigationService.CanGoForward)
            {
                mNavigationService.GoForward();
            }

            UpdateNavigation();
        }

        public void UpdateNavigation()
        {
            BackButton.IsEnabled = mNavigationService.CanGoBack;
            ForwardButton.IsEnabled = mNavigationService.CanGoForward;
        }
    }
}
