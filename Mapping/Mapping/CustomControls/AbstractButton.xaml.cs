using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mapping.CustomControls
{
    /// <summary>
    /// Abstract class to handle styling of the custom buttons throughout the application.
    /// Created by Timothy J Cowen.
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public abstract partial class AbstractButton : UserControl
    {
        #region Property Management

        /// <summary>
        /// Sets up routing the click event to the calling class.
        /// </summary>
        public static RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(AbstractButton));

        /// <summary>
        /// Sets up matching the text property on the button to the specified control.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(AbstractButton));

        /// <summary>
        /// The event handler for the Click event on the button.
        /// </summary>
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        /// <summary>
        /// The property object for the Text field on the button.
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion



        private Brush mCurrentBackground;

        /// <summary>
        /// Constructor.
        /// Sets up custom event handlers.
        /// </summary>
        protected AbstractButton()
        {
            InitializeComponent();
            Loaded += OnLoad;
            IsEnabledChanged += OnChanged_IsEnabled;
        }

        /// <summary>
        /// Sets the background of the button depending on whether the button is enabled or hovered over.
        /// </summary>
        private void UpdateBackground()
        {
            ButtonBackground.Background = !IsEnabled ? Brushes.LightGray : mCurrentBackground;
        }



        #region Custom Event Handlers

        /// <summary>
        /// Listener for when the button is fully loaded.
        /// Updates the background of the button.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnLoad(object sender, EventArgs e)
        {
            mCurrentBackground = GetMainColour();
            UpdateBackground();
        }

        /// <summary>
        /// Listener for when the button is enabled or disabled.
        /// Updates the background of the button.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void OnChanged_IsEnabled(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateBackground();
        }

        #endregion



        #region Overridden Event Handlers

        /// <summary>
        /// Listener for when the mouse hovers over the button.
        /// Updates the background of the button.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            mCurrentBackground = GetHoverColour();
            UpdateBackground();
        }

        /// <summary>
        /// Listener for when the mouse stops hovering over the button.
        /// Updates the background of the button.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            mCurrentBackground = GetMainColour();
            UpdateBackground();
        }

        /// <summary>
        /// Listener for when the mouse stops clicking the button.
        /// Fires the click event to the calling class.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            RoutedEventArgs args = new RoutedEventArgs(ClickEvent, this);
            RaiseEvent(args);
        }

        #endregion



        #region Abstract Methods

        /// <summary>
        /// Allows the extended class to specify the main colour of the button.
        /// </summary>
        /// <returns>The main colour of the button</returns>
        protected abstract Brush GetMainColour();

        /// <summary>
        /// Allows the extended class to specify the hover colour of the button.
        /// </summary>
        /// <returns>The hover colour of the button</returns>
        protected abstract Brush GetHoverColour();

        #endregion
    }
}
