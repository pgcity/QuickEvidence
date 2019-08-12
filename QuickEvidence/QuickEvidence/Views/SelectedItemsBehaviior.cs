using QuickEvidence.ViewModels;
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

        void grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            foreach (var item in dataGrid.Items)
            {
                (item as FileItemViewModel).IsSelected = dataGrid.SelectedItems.Contains(item);
            }
        }
    }
}
