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
                grid.PreparingCellForEdit += Grid_PreparingCellForEdit;
                grid.CellEditEnding += Grid_CellEditEnding;
            }
        }


        protected override void OnDetaching()
        {
            DataGrid grid = (DataGrid)this.AssociatedObject;
            if (grid != null)
            {
                grid.BeginningEdit -= Grid_BeginningEdit;
                grid.PreparingCellForEdit -= Grid_PreparingCellForEdit;
                grid.CellEditEnding -= Grid_CellEditEnding;
            }

            base.OnDetaching();
        }
        
        /// <summary>
        /// 編集開始前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var textEvent = e.EditingEventArgs as System.Windows.Input.TextCompositionEventArgs;
            if (textEvent != null && textEvent.Text != "")
            {
                // テキスト入力による編集開始をさせない
                e.Cancel = true;
                return;
            }

            DataGrid grid = (DataGrid)this.AssociatedObject;
            if(grid.SelectedItems.Count != 1)
            {
                e.Cancel = true;
                return;
            }
            SetIsEditing(grid, true);
        }

        /// <summary>
        /// 編集開始後
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var textBox = e.EditingElement as TextBox;
            var vm = e.Row.DataContext as FileItemViewModel;
            if (textBox != null && vm != null)
            {
                //拡張子を除く部分だけ選択する
                var fileNameWOExt = System.IO.Path.GetFileNameWithoutExtension(textBox.Text);
                textBox.SelectionStart = 0;
                textBox.SelectionLength = fileNameWOExt.Length;
            }
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
            DependencyProperty.RegisterAttached("IsEditing", typeof(bool), typeof(IsEditingBehavior), new PropertyMetadata(false, (sender, arg)=>
            {
                var dataGrid = sender as DataGrid;
                if(dataGrid != null && (bool)arg.NewValue)
                {
                    dataGrid.BeginEdit();
                }
            }));



    }
}
