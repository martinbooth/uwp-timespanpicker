using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace TimespanPicker
{
    internal sealed class TimespanPickerFlyoutPresenter: Control
    {
        private readonly Action dismiss;
        private readonly Action<TimeSpan> accept;
        private readonly int maxHours;
        private readonly int minuteIncrement;
        private readonly LoopingSelector hoursLoopingSelector;
        private readonly LoopingSelector minutesLoopingSelector;
        private ButtonBase acceptButton;
        private ButtonBase dismissButton;

        public TimespanPickerFlyoutPresenter(
            Action dismiss, 
            Action<TimeSpan> accept,
            int maxHours, 
            int minuteIncrement)
        {
            var hourItems = Enumerable.Range(0, maxHours)
                .Select(h => new PickerItem { PrimaryText = h.ToString() })
                .Cast<object>()
                .ToList();
            var minuteItems = Enumerable.Range(0, 60 / minuteIncrement)
                .Select(m => new PickerItem { PrimaryText = (m * minuteIncrement).ToString("D2") })
                .Cast<object>()
                .ToList();

            this.DefaultStyleKey = typeof(TimespanPickerFlyoutPresenter);
            this.dismiss = dismiss;
            this.accept = accept;
            this.maxHours = maxHours;
            this.minuteIncrement = minuteIncrement;
            this.hoursLoopingSelector = new LoopingSelector { Items = hourItems };
            this.minutesLoopingSelector = new LoopingSelector { Items = minuteItems};
        }

        public static TimeSpan RoundToNearest(TimeSpan timespan, TimeSpan interval)
        {
            var delta = timespan.Ticks % interval.Ticks;
            bool roundUp = delta > interval.Ticks / 2;
            var offset = roundUp ? interval.Ticks : 0;

            return new TimeSpan(timespan.Ticks + offset - delta);
        }

        internal void SetValue(TimeSpan value)
        {
            var roundedValue = RoundToNearest(value, TimeSpan.FromMinutes(this.minuteIncrement));

            if (roundedValue.TotalHours > this.maxHours)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value exceeds maximum timespan selectable");
            }

            this.hoursLoopingSelector.SelectedIndex = (int)roundedValue.TotalHours;
            this.minutesLoopingSelector.SelectedIndex = roundedValue.Minutes / this.minuteIncrement;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var firstPickerHostBorder = this.GetTemplateChild("FirstPickerHost") as Border;
            firstPickerHostBorder.Child = this.hoursLoopingSelector;

            var secondPickerHostBorder = this.GetTemplateChild("SecondPickerHost") as Border;
            secondPickerHostBorder.Child = this.minutesLoopingSelector;

            if (this.acceptButton != null)
            {
                this.acceptButton.Click -= this.AcceptButtonClick;
            }

            if (this.dismissButton != null)
            {
                this.dismissButton.Click -= this.DismissButtonClick;
            }

            this.acceptButton = this.GetTemplateChild("AcceptButton") as ButtonBase;
            this.dismissButton = this.GetTemplateChild("DismissButton") as ButtonBase;

            this.acceptButton.Click += this.AcceptButtonClick;
            this.dismissButton.Click += this.DismissButtonClick;
        }

        private void DismissButtonClick(object sender, RoutedEventArgs e)
        {
            this.dismiss();
        }

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            this.accept(new TimeSpan(
                (int)this.hoursLoopingSelector.SelectedIndex,
                (int)this.minutesLoopingSelector.SelectedIndex * this.minuteIncrement,
                0));
        }
    }
}
