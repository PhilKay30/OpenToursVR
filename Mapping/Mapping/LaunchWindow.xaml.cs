using Mapping.Common;
using System.Windows;
using System.Windows.Navigation;

namespace Mapping
{
    // ReSharper disable once RedundantExtendsListEntry
    public partial class LaunchWindow : Window
    {
        private readonly NavigationService mNavigationService;

        public LaunchWindow()
        {
            InitializeComponent();
            mNavigationService = ContentFrame.NavigationService;
            UpdateNavigation();

            // Attempt to load configuration
            ConfigInterface.ConfigStatus status = ConfigInterface.LoadConfig();
            string errorMessage;

            // Get result of configuration load
            switch (status)
            {
                case ConfigInterface.ConfigStatus.OK:
                    return;
                case ConfigInterface.ConfigStatus.ConfigDoesNotExist:
                    errorMessage = "The configuration file does not exist." +
                                   "\nYou must create a configuration file to proceed.";
                    break;
                case ConfigInterface.ConfigStatus.InvalidBaseTag:
                    errorMessage = "The configuration file must start with the <config> tag." +
                                   "\nWould you like to replace the file with a new one?";
                    break;
                case ConfigInterface.ConfigStatus.DatabaseTagDoesNotExist:
                    errorMessage = "The configuration file must include the <database> tag." +
                                   "\nWould you like to replace the file with a new one?";
                    break;
                case ConfigInterface.ConfigStatus.ApiTagDoesNotExist:
                    errorMessage = "The configuration file must include the <api> tag." +
                                   "\nWould you like to replace the file with a new one?";
                    break;
                case ConfigInterface.ConfigStatus.DatabaseTagIsMissingField:
                    errorMessage = "The database configuration is missing a field." +
                                   "\nWould you like to replace the file with a new one?";
                    break;
                case ConfigInterface.ConfigStatus.ApiTagIsMissingField:
                    errorMessage = "The api configuration is missing a field." +
                                   "\nWould you like to replace the file with a new one?";
                    break;
                case ConfigInterface.ConfigStatus.ErrorLoadingConfig:
                case ConfigInterface.ConfigStatus.Unknown:
                default:
                    errorMessage = "Something went wrong loading the configuration file." +
                                   "\nWould you like to replace the file with a new one?";
                    break;
            }

            // Display error
            MessageBoxResult result = MessageBox.Show(
                errorMessage,
                "Configuration Error",
                MessageBoxButton.YesNo);

            // Check if user has requested to regenerate the config
            if (result == MessageBoxResult.Yes)
            {
                // Generate a new configuration
                ConfigInterface.GenerateEmptyConfigFile(FileIO.GetConfigDirectory() + "\\config.xml");
            }

            // Inform the user to fix the configuration file
            MessageBox.Show(
                "Please take a moment to fix the configuration file.",
                "Configuration Needs Attention");

            // Close the application
            Application.Current.Shutdown();
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
