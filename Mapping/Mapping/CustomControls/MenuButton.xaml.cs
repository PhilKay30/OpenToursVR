using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mapping.CustomControls
{
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MenuButton : UserControl
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
                    GridMain.Background = _currentBackground;
                }
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(MenuButton));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty MainColourProperty = DependencyProperty.Register(
            "MainColour",
            typeof(Brush),
            typeof(MenuButton),
            new PropertyMetadata(Brushes.Lavender));

        public static readonly DependencyProperty HoverColourProperty = DependencyProperty.Register(
            "HoverColour",
            typeof(Brush),
            typeof(MenuButton),
            new PropertyMetadata(Brushes.Plum));

        public Brush MainColour
        {
            get => (Brush)GetValue(MainColourProperty);
            set => SetValue(MainColourProperty, value);
        }

        public Brush HoverColour
        {
            get => (Brush)GetValue(HoverColourProperty);
            set => SetValue(HoverColourProperty, value);
        }


        public MenuButton()
        {
            InitializeComponent();
            IsEnabledChanged += OnChanged_IsEnabled;
            HandleEnabledChanged();
            CurrentBackground = MainColour;
        }

        private void HandleEnabledChanged()
        {
            if (!IsEnabled)
            {
                GridMain.Background = sDisabledBackground;
            }
        }


        private void OnChanged_IsEnabled(object sender, DependencyPropertyChangedEventArgs e)
        {
            HandleEnabledChanged();
        }



        public static RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(MenuButton));

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            CurrentBackground = HoverColour;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            CurrentBackground = MainColour;
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
