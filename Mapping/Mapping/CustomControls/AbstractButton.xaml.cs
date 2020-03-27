using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mapping.CustomControls
{
    // ReSharper disable once RedundantExtendsListEntry
    public abstract partial class AbstractButton : UserControl
    {
        private static readonly Brush sDisabledBackground = Brushes.LightGray;

        private Brush _currentBackground;
        private Brush CurrentBackground
        {
            get => _currentBackground;
            set
            {
                _currentBackground = value;
                if (IsEnabled)
                {
                    ButtonBackground.Background = CurrentBackground;
                }
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(AbstractButton));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }


        protected AbstractButton()
        {
            InitializeComponent();
            IsEnabledChanged += OnChanged_IsEnabled;
            HandleEnabledChanged();
            CurrentBackground = GetMainColour();
        }

        protected abstract Brush GetMainColour();
        protected abstract Brush GetHoverColour();

        private void HandleEnabledChanged()
        {
            ButtonBackground.Background = !IsEnabled ? sDisabledBackground : CurrentBackground;
        }


        private void OnChanged_IsEnabled(object sender, DependencyPropertyChangedEventArgs e)
        {
            HandleEnabledChanged();
        }



        public static RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(AbstractButton));

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            CurrentBackground = GetHoverColour();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            CurrentBackground = GetMainColour();
        }

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        protected virtual void OnClick()
        {
            RoutedEventArgs args = new RoutedEventArgs(ClickEvent, this);
            RaiseEvent(args);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            OnClick();
        }
    }
}
