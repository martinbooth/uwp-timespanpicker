using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace TimespanPicker
{
    public sealed class TimespanPicker : Control, INotifyPropertyChanged
    {
        private FrameworkElement layoutRoot;
        private ButtonBase flyoutButton;
        private TimespanPickerFlyout timespanPickerFlyout;
        private TextBlock timespanTextBlock;
        private ButtonBase deleteButton;

        public event EventHandler<TimespanPickerValueChangedEventArgs> TimespanChanged;
        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(nameof(TimeSpan));
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SetTimespan(TimeSpan? timeSpan)
        {
            if (this.timespanTextBlock != null)
            {
                this.timespanTextBlock.Text = timeSpan == null ? string.Empty : this.FormatValue(timeSpan.Value);
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
                this.flyoutButton.Click -= this.FlyoutButtonClick;
            }

            if (this.deleteButton != null)
            {
                this.deleteButton.Click -= this.DeleteButtonClick;
            }

            this.layoutRoot = GetTemplateChild("LayoutRoot") as FrameworkElement;
            this.flyoutButton = GetTemplateChild("FlyoutButton") as ButtonBase;
            this.timespanTextBlock = GetTemplateChild("TimespanTextBlock") as TextBlock;
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
            this.timespanPickerFlyout.ShowAt(this.flyoutButton);
        }

        private string FormatValue(TimeSpan value)
        {
            var parts = new[] {
                new { Value = value.Days, Text = "days" },
                new { Value = value.Hours, Text= "hrs" },
                new { Value = value.Minutes, Text = "mins" },
                new { Value = value.Seconds, Text = "secs" }
            };

            return string.Join(" ", from p in parts
                                    where p.Value != 0
                                    select string.Format("{0} {1}", p.Value, p.Text));
        }
    }
}
