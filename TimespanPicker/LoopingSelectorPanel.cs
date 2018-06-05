using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace TimespanPicker
{
    public sealed class LoopingSelectorPanel : Canvas, IScrollSnapPointsInfo
    {
        private LoopingSelector loopingSelector;
        private Dictionary<int, LoopingSelectorItem> prevItems = new Dictionary<int, LoopingSelectorItem>();

        public LoopingSelectorPanel(LoopingSelector loopingSelector)
        {
            this.loopingSelector = loopingSelector;
        }

        internal void InvalidateItems()
        {
            foreach (var item in this.prevItems.Values)
            {
                this.UpdateItem(item);
            }
        }

        private LoopingSelectorItem UpdateItem(LoopingSelectorItem item)
        {
            item.Height = this.loopingSelector.ItemHeight;
            item.Width = this.loopingSelector.ActualWidth;
            item.ContentTemplate = this.loopingSelector.ItemTemplate;
            return item;
        }

        internal void Layout(double viewportTop, double viewportHeight)
        {
            var itemHeight = this.loopingSelector.ItemHeight;
            var startItemIndex = (int)Math.Max(0, Math.Floor(viewportTop / itemHeight));
            var totalItems = this.loopingSelector.Items.Count;
            var newIndexes = Enumerable.Range(startItemIndex, (int)(viewportHeight / itemHeight));
            var prevNewIndexesIntersect = new HashSet<int>(newIndexes.Where(i => this.prevItems.ContainsKey(i)));
            var prevItemsNotInNew = new Queue<LoopingSelectorItem>(
                this.prevItems.Keys
                    .Where(k => !prevNewIndexesIntersect.Contains(k))
                    .Select(k => this.prevItems[k])
            );

            var newItemsNotInPrev = newIndexes
                .Where(i => !this.prevItems.ContainsKey(i))
                .ToDictionary(
                    i => i,
                    i =>
                    {
                        var child = prevItemsNotInNew.Count == 0 ?
                            this.UpdateItem(new LoopingSelectorItem(this.loopingSelector)) :
                            prevItemsNotInNew.Dequeue();

                        child.VirtualIndex = i;
                        child.Content = this.loopingSelector.Items[child.VirtualIndex % totalItems];
                        child.SetValue(LeftProperty, 0);
                        child.SetValue(TopProperty, i * itemHeight);
                        return child;
                    }
                );

            foreach (var item in prevItemsNotInNew)
            {
                this.Children.Remove(item);
            }

            foreach (var item in newItemsNotInPrev.Values)
            {
                if (item.Parent == null) {
                    this.Children.Add(item);
                }
            }

            var items = newItemsNotInPrev;
            foreach (var index in prevNewIndexesIntersect)
            {
                items[index] = this.prevItems[index]; 
            }

            this.prevItems = items;
        }

        public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
        {
            throw new NotImplementedException();
        }

        public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
        {
            offset = (float)this.loopingSelector.ItemOffset;
            return this.loopingSelector.ItemHeight;
        }

        public bool AreHorizontalSnapPointsRegular => true;

        public bool AreVerticalSnapPointsRegular => true;

        public event EventHandler<object> HorizontalSnapPointsChanged;
        public event EventHandler<object> VerticalSnapPointsChanged;
    }
}
