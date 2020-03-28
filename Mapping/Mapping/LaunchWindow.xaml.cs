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
            // ReSharper disable RedundantCaseLabel
            // ReSharper disable PossibleNullReferenceException
            switch (status)
            {
                case ConfigInterface.ConfigStatus.OK:
                    return;
                case ConfigInterface.ConfigStatus.ConfigDoesNotExist:
                    errorMessage = Application.Current.FindResource("PromptConfigFileDoesNotExist")?.ToString();
                    break;
                case ConfigInterface.ConfigStatus.InvalidBaseTag:
                    errorMessage = Application.Current.FindResource("PromptConfigBaseTagInvalid")?.ToString();
                    break;
                case ConfigInterface.ConfigStatus.DatabaseTagDoesNotExist:
                    errorMessage = string.Format(Application.Current.FindResource("PromptConfigTagMissing").ToString(), @"database");
                    break;
                case ConfigInterface.ConfigStatus.ApiTagDoesNotExist:
                    errorMessage = string.Format(Application.Current.FindResource("PromptConfigTagMissing").ToString(), @"api");
                    break;
                case ConfigInterface.ConfigStatus.MapTagDoesNotExist:
                    errorMessage = string.Format(Application.Current.FindResource("PromptConfigTagMissing").ToString(), @"map");
                    break;
                case ConfigInterface.ConfigStatus.DatabaseTagIsMissingField:
                    errorMessage = string.Format(Application.Current.FindResource("PromptConfigFieldInvalid").ToString(), @"database");
                    break;
                case ConfigInterface.ConfigStatus.ApiTagIsMissingField:
                    errorMessage = string.Format(Application.Current.FindResource("PromptConfigFieldInvalid").ToString(), @"api");
                    break;
                case ConfigInterface.ConfigStatus.MapTagIsMissingField:
                    errorMessage = string.Format(Application.Current.FindResource("PromptConfigFieldInvalid").ToString(), @"map");
                    break;
                case ConfigInterface.ConfigStatus.ErrorLoadingConfig:
                case ConfigInterface.ConfigStatus.Unknown:
                default:
                    errorMessage = Application.Current.FindResource("PromptConfigSomethingWentWrong")?.ToString();
                    break;
            }

            // Display error
            MessageBoxResult result = MessageBox.Show(
                errorMessage,
                Application.Current.FindResource("PromptConfigTitle")?.ToString(),
                MessageBoxButton.YesNo);

            // Check if user has requested to regenerate the config
            if (result == MessageBoxResult.Yes)
            {
                // Generate a new configuration
                ConfigInterface.GenerateEmptyConfigFile(FileIO.GetConfigDirectory() + "\\config.xml");
            }

            // Inform the user to fix the configuration file
            MessageBox.Show(
                Application.Current.FindResource("PromptConfigNeedsAttention")?.ToString(),
                Application.Current.FindResource("PromptConfigTitle")?.ToString());

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
