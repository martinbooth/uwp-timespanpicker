using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace TimespanPicker
{
    internal class FlyoutBase: DependencyObject
    {
        private Popup popup;
        private Control presenter;
        private FrameworkElement placementTarget;

        public event EventHandler<object> Opened;

        public FlyoutBase()
        {
            this.popup = new Popup
            {
                IsLightDismissEnabled = true
            };

            this.presenter = this.CreatePresenter();
        }

        internal virtual void ShowAt(FrameworkElement placementTarget)
        {
            var placementTargetToScreenTransform = placementTarget.TransformToVisual(Window.Current.Content);
            var placementTargetPosition = placementTargetToScreenTransform.TransformPoint(new Point(0, 0));
            this.placementTarget = placementTarget;
            this.popup.Child = this.presenter;
            this.popup.LayoutUpdated += PopupLayoutUpdated;
            this.popup.Opened += PopupOpened;
            this.popup.Closed += PopupClosed;
            this.popup.IsOpen = true;
            this.popup.VerticalOffset = placementTargetPosition.Y;
            this.popup.HorizontalOffset = placementTargetPosition.X;
        }

        protected virtual Transform GetTransform(Control presenter, FrameworkElement placementTarget)
        {
            return null;
        }

        private void PopupLayoutUpdated(object sender, object e)
        {
            if (this.placementTarget != null && this.placementTarget != null)
            {
                this.presenter.RenderTransform = this.GetTransform(this.presenter, this.placementTarget);
            }
        }

        private void PopupOpened(object sender, object e)
        {
            this.Opened?.Invoke(this, EventArgs.Empty);
        }

        private void PopupClosed(object sender, object e)
        {
            this.popup.Child = null;
            this.placementTarget = null;
        }

        public void Hide()
        {
            this.popup.IsOpen = false;
        }

        protected virtual Control CreatePresenter()
        {
            return null;
        }
    }
}
