using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace Mapping.HistMapConfig
{
    /// <summary>
    /// Interaction logic for HistMapConfigPage.xaml
    /// </summary>
    public partial class HistMapConfigPage : Page
    {
        /// <summary>
        /// External method call to Windows SDK
        /// This is necessary but won't be called as movement of window is disabled
        /// </summary>
        /// <param name="handle">handle to use for window binding</param>
        /// <param name="x">start at x</param>
        /// <param name="y">start at y</param>
        /// <param name="width">width of window</param>
        /// <param name="height">height of window</param>
        /// <param name="redraw">to redraw on fresh load?</param>
        /// <returns>true upon successful window move</returns>
        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        /// <summary>
        /// proc catch for windows SDK enumeration (it's important but ignore it, no touchy)
        /// </summary>
        /// <param name="hwnd">windows handle for binding</param>
        /// <param name="lparam">param data for the SDK call</param>
        /// <returns>true upon successful binding</returns>
        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);

        /// <summary>
        /// enternal SDK call to enumerate children (windows) acting as embedded windows apps
        /// </summary>
        /// <param name="hwnd">windows handle for binding</param>
        /// <param name="func">function to enumerate children</param>
        /// <param name="lParam">param data for the SDK call</param>
        /// <returns>true upon successful binding</returns>
        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

        /// <summary>
        /// external method to send message to windows SDK
        /// </summary>
        /// <param name="hWnd">handle that called for msg send</param>
        /// <param name="msg">the message</param>
        /// <param name="wParam">initial param for SDK call</param>
        /// <param name="lParam">additional param data for SDK call</param>
        /// <returns>number of sent messages (should be 1)</returns>
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The process for the Unity app
        /// </summary>
        private Process process;

        /// <summary>
        /// the unity apps window handle for binding purposes
        /// </summary>
        private IntPtr unityHWND = IntPtr.Zero;

        /// <summary>
        /// Windows specific bindings for readability purposes
        /// </summary>
        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);
        private readonly IntPtr WA_INACTIVE = new IntPtr(0);

        /// <summary>
        /// Page Constructor
        /// </summary>
        public HistMapConfigPage()
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
            // update navigation pane and start the unity app up...
            (System.Windows.Application.Current.MainWindow as LaunchWindow)?.UpdateNavigation();
            RunUnityApp();
        }


        /// <summary>
        /// This method starts the embedded unity application
        /// </summary>
        private void RunUnityApp()
        {
            // create the unity app process and give it the Forms panel handle as an arg (unity requirement)
            // then start the process
            process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\..\\HistMapConfig\\UnityTool\\MapConfig.exe";
            process.StartInfo.Arguments = "-parentHWND " + MyPanel.Handle.ToInt32() + " " + Environment.CommandLine;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            // wait until app is done, check for other apps needing to start (should not
            //  happen in this implementation but it's best pactice and WPF seems to prefer it)
            process.WaitForInputIdle();
            EnumChildWindows(MyPanel.Handle, WindowEnum, IntPtr.Zero);
        }


        /// <summary>
        /// This method notifies the windows SDK of the application's start
        /// </summary>
        private void ActivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
        }

        /// <summary>
        /// This method notifies the windows SDK of the application's end
        /// </summary>
        private void DeactivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
        }

        /// <summary>
        /// This method forces 1 unity app to start and nothing else (should attempt
        /// a restart upon crash)
        /// </summary>
        /// <param name="hwnd">handle for the window</param>
        /// <param name="lparam">param data for the SDK</param>
        /// <returns></returns>
        private int WindowEnum(IntPtr hwnd, IntPtr lparam)
        {
            unityHWND = hwnd;
            ActivateUnityWindow();
            return 0;
        }

        /// <summary>
        /// This method turns the resize attempt into a rebind to the initial placement
        /// basically holds the unity app in place
        /// </summary>
        /// <param name="sender">caller</param>
        /// <param name="e">what the caller said</param>
        private void panel1_Resize(object sender, EventArgs e)
        {
            MoveWindow(unityHWND, 0, 0, MyPanel.Width, MyPanel.Height, true);
            ActivateUnityWindow();
        }

        /// <summary>
        /// This method closes the Unity app
        /// (this will allow process to end properly if page is closed / user navigates away)
        /// </summary>
        /// <param name="sender">caller</param>
        /// <param name="e">what the caller said</param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                // close the window, wait a sec to see if app dies
                // attempt to kill it after it closed (cuz we can't see if it's secretly a zombie)
                process.CloseMainWindow();
                Thread.Sleep(1000);
                while (!process.HasExited)
                    process.Kill();
            }
            catch (Exception)
            {
                // this catches a failed kill if the app exited properly (or was already done upon exit)
            }
        }

        /// <summary>
        /// Pretty sure this isn't needed here but hey, what's an extra method as a forms fail-safe?
        /// </summary>
        /// <param name="sender">caller</param>
        /// <param name="e">what the caller said</param>
        private void Form1_Activated(object sender, EventArgs e)
        {
            ActivateUnityWindow();
        }


        /// <summary>
        /// Pretty sure this isn't needed here but hey, what's an extra method as a forms fail-safe?
        /// </summary>
        /// <param name="sender">caller</param>
        /// <param name="e">what the caller said</param>
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            DeactivateUnityWindow();
        }
    }
}
