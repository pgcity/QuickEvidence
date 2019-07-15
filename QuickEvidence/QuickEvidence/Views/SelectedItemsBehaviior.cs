using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace QuickEvidence.Views
{
    public class SelectedItemsBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            DataGrid grid = (DataGrid)this.AssociatedObject;
            if (grid != null)
            {
                grid.SelectionChanged += grid_SelectionChanged;
            }
        }

        protected override void OnDetaching()
        {
            DataGrid grid = (DataGrid)this.AssociatedObject;
            if (grid != null)
            {
                grid.SelectionChanged -= grid_SelectionChanged;
            }

            base.OnDetaching();
        }

        public static DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(SelectedItemsBehavior), new PropertyMetadata(null));

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        void grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //新規選択されたアイテムをリストに追加
            foreach (var addedItem in e.AddedItems)
            {
                SelectedItems?.Add(addedItem);
            }

            //選択解除されたアイテムをリストから削除
            foreach (var removedItem in e.RemovedItems)
            {
                SelectedItems?.Remove(removedItem);
            }
        }
    }
}
