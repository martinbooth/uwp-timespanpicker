using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace TimespanPicker
{
    public sealed class LoopingSelector : Control
    {
        private ScrollViewer scrollViewer;
        private LoopingSelectorPanel loopingSelectorPanel;
        private int selectedVirtualIndex;

        public static DependencyProperty ItemsProperty { get; } = DependencyProperty.Register(
            "Items",
            typeof(IList<object>),
            typeof(LoopingSelector),
            new PropertyMetadata(new List<object>(), (d, e) => ((LoopingSelector)d).LayoutPanel()));

        public static DependencyProperty ItemTemplateProperty { get; } = DependencyProperty.Register(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(LoopingSelector),
            new PropertyMetadata(null));

        public static DependencyProperty ItemHeightProperty { get; } = DependencyProperty.Register(
           "ItemHeight",
           typeof(int),
           typeof(LoopingSelector),
           new PropertyMetadata(44));

        public IList<object> Items
        {
            get => (IList<object>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }


        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public int ItemHeight
        {
            get => (int)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        internal int TotalListHeight { get => this.ItemHeight * this.Items.Count; }
        internal double ItemOffset { get => (this.ActualHeight - this.ItemHeight) / 2; }
        internal double RepeatCount { get => this.loopingSelectorPanel.Height / this.TotalListHeight; }

        public LoopingSelector()
        {
            this.DefaultStyleKey = typeof(LoopingSelector);
            this.loopingSelectorPanel = new LoopingSelectorPanel(this) { Height = 44298 };
        }

        internal void LoopingSelectorItemPressed(double offset)
        {
            this.scrollViewer.ChangeView(null, offset - this.ItemOffset, null);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var upButton = (ButtonBase)this.GetTemplateChild("UpButton");
            var downButton = (ButtonBase)this.GetTemplateChild("DownButton");

            if (this.scrollViewer != null)
            {
                this.scrollViewer.ViewChanged -= ScrollViewerViewChanged;
                this.scrollViewer.SizeChanged -= ScrollViewerSizeChanged;
                this.scrollViewer.Loaded -= ScrollViewerLoaded;
                upButton.Click -= UpButtonClicked;
                downButton.Click -= DownButtonClicked;
            }

            this.scrollViewer = (ScrollViewer)this.GetTemplateChild("ScrollViewer");
            this.scrollViewer.Content = this.loopingSelectorPanel;

            this.scrollViewer.ViewChanged += ScrollViewerViewChanged;
            this.scrollViewer.SizeChanged += ScrollViewerSizeChanged;
            this.scrollViewer.Loaded += ScrollViewerLoaded;

            upButton.Click += UpButtonClicked;
            downButton.Click += DownButtonClicked;

            this.LayoutPanel();
        }

        private void ScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            this.SelectVirtualIndex(this.SelectedVirtualIndex);
        }

        private void UpButtonClicked(object sender, RoutedEventArgs e)
        {
            this.scrollViewer.ChangeView(null, (this.SelectedVirtualIndex - 1) * this.ItemHeight - this.ItemOffset, null, false);
        }

        private void DownButtonClicked(object sender, RoutedEventArgs e)
        {
            this.scrollViewer.ChangeView(null, (this.SelectedVirtualIndex + 1) * this.ItemHeight - this.ItemOffset, null, false);
        }

        private void SelectVirtualIndex(int selectedVirtualIndex)
        {
            if (this.scrollViewer == null)
            {
                return;
            }

            var verticalOffset = selectedVirtualIndex * this.ItemHeight - this.ItemOffset;
            this.scrollViewer.ChangeView(null, verticalOffset, null, true);
        }

        private void ScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.LayoutPanel();
            this.loopingSelectorPanel.InvalidateItems();
        }

        private void LayoutPanel()
        {
            if (this.scrollViewer == null || this.Items == null)
            {
                return;
            }

            this.loopingSelectorPanel.Layout(
                this.scrollViewer.VerticalOffset - this.ItemHeight * 4,
                this.scrollViewer.ActualHeight + this.ItemHeight * 8
            );
        }

        private static double DMod(double a, double b)
        {
            return a - b * Math.Floor(a / b);
        }

        private void ScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var repeatIndex = Math.Floor(this.RepeatCount / 2);
            var min = (repeatIndex - 1) * Items.Count - 0.5;
            var max = (repeatIndex + 1) * Items.Count - 0.5;

            var selectedVirtualIndex = (this.scrollViewer.VerticalOffset + this.ItemOffset) / this.ItemHeight;

            if (selectedVirtualIndex >= max || selectedVirtualIndex < min)
            {
                this.scrollViewer.ChangeView(
                    null,
                    (repeatIndex * Items.Count + (DMod(selectedVirtualIndex + 0.5, this.Items.Count) - 0.5)) * this.ItemHeight - this.ItemOffset,
                    null,
                    true);
            }

            this.selectedVirtualIndex = (int)Math.Round(selectedVirtualIndex);

            this.LayoutPanel();
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }

        public object SelectedItem
        {
            get => this.Items[this.SelectedIndex];
            set => this.SelectedIndex = this.Items.IndexOf(value);
        }

        public int SelectedIndex
        {
            get => this.SelectedVirtualIndex % this.Items.Count;
            set => this.SelectedVirtualIndex = (int)(this.RepeatCount / 2) * this.Items.Count + value;
        }

        internal int SelectedVirtualIndex
        {
            get => this.selectedVirtualIndex;

            set
            {
                this.selectedVirtualIndex = value;
                this.SelectVirtualIndex(value);
            }
        }
    }
}
