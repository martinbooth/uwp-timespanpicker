using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace TimespanPicker
{
    public sealed class LoopingSelectorItem : ContentControl
    {
        private LoopingSelector loopingSelector;

        public LoopingSelectorItem(LoopingSelector loopingSelector)
        {
            this.loopingSelector = loopingSelector;
            this.DefaultStyleKey = typeof(LoopingSelectorItem);
        }

        internal int VirtualIndex { get; set; }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);

            if (this.VirtualIndex != this.loopingSelector.SelectedVirtualIndex) {
                VisualStateManager.GoToState(this, "PointerOver", true);
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);

            VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (this.VirtualIndex != this.loopingSelector.SelectedVirtualIndex)
            {
                VisualStateManager.GoToState(this, "Pressed", true);
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            VisualStateManager.GoToState(this, "Normal", true);

            this.loopingSelector.LoopingSelectorItemPressed((double)GetValue(Canvas.TopProperty));
        }
    }
}