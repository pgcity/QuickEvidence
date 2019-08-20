using QuickEvidence.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace QuickEvidence.Views
{
    public class IsEditingBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            DataGrid grid = (DataGrid)this.AssociatedObject;
            if (grid != null)
            {
                grid.BeginningEdit += Grid_BeginningEdit;
                grid.CellEditEnding += Grid_CellEditEnding;
            }
        }

        protected override void OnDetaching()
        {
            DataGrid grid = (DataGrid)this.AssociatedObject;
            if (grid != null)
            {
                grid.BeginningEdit -= Grid_BeginningEdit;
                grid.CellEditEnding -= Grid_CellEditEnding;
            }

            base.OnDetaching();
        }
        
        /// <summary>
        /// 編集開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            DataGrid grid = (DataGrid)this.AssociatedObject;
            SetIsEditing(grid, true);
        }

        /// <summary>
        /// 編集終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            DataGrid grid = (DataGrid)this.AssociatedObject;
            SetIsEditing(grid, false);
        }

        /// <summary>
        /// 編集中かどうかを表すプロパティ
        /// </summary>
        public static bool GetIsEditing(DependencyObject obj)
        {
            var control = obj as Control;
            return (bool)control.GetValue(IsEditingProperty);
        }

        public static void SetIsEditing(DependencyObject obj, bool value)
        {
            var control = obj as Control;
            control.SetValue(IsEditingProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsEditing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.RegisterAttached("IsEditing", typeof(bool), typeof(IsEditingBehavior), new PropertyMetadata());



    }
}
