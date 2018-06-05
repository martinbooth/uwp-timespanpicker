using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TimespanPicker
{
    internal class TimespanPickerFlyout : FlyoutBase
    {
        public static DependencyProperty TimeSpanProperty { get; } = DependencyProperty.Register(
            "TimeSpan",
            typeof(TimeSpan),
            typeof(TimespanPickerFlyout),
            new PropertyMetadata(TimeSpan.Zero));

        private TimespanPickerFlyoutPresenter timespanPickerFlyoutPresenter;

        public TimeSpan TimeSpan
        {
            get => (TimeSpan)GetValue(TimeSpanProperty);
            set => SetValue(TimeSpanProperty, value);
        }

        public TimespanPickerFlyout()
        {
            this.Opened += this.FlyoutOpened;
        }

        protected override Transform GetTransform(Control presenter, FrameworkElement placementTarget)
        {
            var top = -(presenter.RenderSize.Height - placementTarget.RenderSize.Height) / 2;
            var placementTargetToScreenTransform = placementTarget.TransformToVisual(Window.Current.Content);
            var placementTargetPosition = placementTargetToScreenTransform.TransformPoint(new Point(0, 0));

            if (top + placementTargetPosition.Y < 0)
            {
                top = -placementTargetPosition.Y;
            }

            if (top + placementTargetPosition.Y + presenter.RenderSize.Height >= Window.Current.Bounds.Height)
            {
                top = Window.Current.Bounds.Height - presenter.RenderSize.Height - placementTargetPosition.Y;
            }

            return new TranslateTransform
            {
                Y = top
            };
        }

        private void FlyoutOpened(object sender, object e)
        {
            this.timespanPickerFlyoutPresenter.SetValue(this.TimeSpan);
        }

        protected override Control CreatePresenter()
        {
            this.timespanPickerFlyoutPresenter = new TimespanPickerFlyoutPresenter(this.Hide, value =>
            {
                this.TimeSpan = value;
                this.Hide();
            }, 24, 5);

            return this.timespanPickerFlyoutPresenter;
        }
    }
}