using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace TimespanPicker
{
    public sealed class TimespanPicker : Control
    {
        private FrameworkElement layoutRoot;
        private ButtonBase flyoutButton;
        private TextBlock minuteTextBlock;
        private TimespanPickerFlyout timespanPickerFlyout;
        private TextBlock hourTextBlock;
        private ButtonBase deleteButton;

        public event EventHandler<TimespanPickerValueChangedEventArgs> TimespanChanged;

        public static DependencyProperty FooterTemplateProperty { get; } = DependencyProperty.Register(
            "FooterTemplate",
            typeof(DataTemplate),
            typeof(TimespanPicker),
            new PropertyMetadata(null, LoadFooter));

        public static DependencyProperty FooterProperty { get; } = DependencyProperty.Register(
            "Footer",
            typeof(object),
            typeof(TimespanPicker),
            new PropertyMetadata(null, LoadFooter));

        public static DependencyProperty TimespanProperty { get; } = DependencyProperty.Register(
            "Timespan",
            typeof(TimeSpan?),
            typeof(TimespanPicker),
            new PropertyMetadata(null, OnTimespanPropertyChanged));

        private static void OnTimespanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timespanPicker = (TimespanPicker)d;
            timespanPicker.TimespanChanged?.Invoke(
                timespanPicker,
                new TimespanPickerValueChangedEventArgs
                {
                    NewTimespan = (TimeSpan?)e.NewValue,
                    OldTimespan = (TimeSpan?)e.OldValue
                });
        }

        public DataTemplate FooterTemplate
        {
            get => (DataTemplate)GetValue(FooterTemplateProperty);
            set => SetValue(FooterTemplateProperty, value);
        }

        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }

        public TimeSpan? TimeSpan
        {
            get => (TimeSpan?)GetValue(TimespanProperty);

            set
            {
                SetValue(TimespanProperty, value);
                this.SetTimespan(value);
            }
        }

        private void SetTimespan(TimeSpan? timeSpan)
        {
            if (this.hourTextBlock != null)
            {
                this.hourTextBlock.Text = timeSpan == null ? string.Empty : timeSpan.Value.TotalHours.ToString("F0");
            }

            if (this.minuteTextBlock != null)
            {
                this.minuteTextBlock.Text = timeSpan == null ? string.Empty : timeSpan.Value.Minutes.ToString("F0");
            }
        }

        private static void LoadFooter(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timespanPicker = (TimespanPicker)d;

            if (timespanPicker.GetTemplateChild("FooterContentPresenter") is FrameworkElement footerContentPresenter)
            {
                footerContentPresenter.Visibility = Visibility.Visible;
            }
        }

        public TimespanPicker()
        {
            this.DefaultStyleKey = typeof(TimespanPicker);
            this.timespanPickerFlyout = new TimespanPickerFlyout();

            BindingOperations.SetBinding(
                this.timespanPickerFlyout,
                TimespanPickerFlyout.TimeSpanProperty,
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath("TimeSpan"),
                    Mode = BindingMode.TwoWay
                });
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.Footer != null || this.FooterTemplate != null)
            {
                LoadFooter(this, null);
            }

            if (this.flyoutButton != null)
            {
                this.flyoutButton.Click += this.FlyoutButtonClick;
            }

            if (this.deleteButton != null)
            {
                this.deleteButton.Click += this.DeleteButtonClick;
            }

            this.layoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;
            this.flyoutButton = GetTemplateChild("FlyoutButton") as ButtonBase;
            this.minuteTextBlock = GetTemplateChild("MinuteTextBlock") as TextBlock;
            this.hourTextBlock = GetTemplateChild("HourTextBlock") as TextBlock;
            this.deleteButton = GetTemplateChild("DeleteButton") as ButtonBase;
            this.SetTimespan(this.TimeSpan);

            this.flyoutButton.Click += this.FlyoutButtonClick;
            this.deleteButton.Click += this.DeleteButtonClick;
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            this.TimeSpan = null;
        }

        private void FlyoutButtonClick(object sender, RoutedEventArgs e)
        {
            this.timespanPickerFlyout.ShowAt(this);
        }
    }
}
