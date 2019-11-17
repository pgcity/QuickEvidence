using QuickEvidence.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QuickEvidence.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, IGetPosition, IColorDialog, ITextInputWindow, IDataGrid, INavigation
    {
        public MainWindow()
        {
            InitializeComponent();

            MainWindowViewModel vm = (MainWindowViewModel)DataContext;
            vm.GetPositionIF = this;
            vm.ColorDialogIF = this;
            vm.TextInputWindowIF = this;
            vm.DataGridIF = this;
            vm.NavigationIF = this;
        }

        /// <summary>
        /// DataGridの編集をキャンセル
        /// </summary>
        public void CancelEdit()
        {
            fileListDataGrid.CancelEdit();
        }

        /// <summary>
        /// DataGridの編集をコミット
        /// </summary>
        public void CommitEdit()
        {
            fileListDataGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);
        }

        public Point GetPositionFromScrollViewer(MouseEventArgs arg)
        {
            return arg.GetPosition(mainScrollViewer);
        }

        /// <summary>
        /// オフセットを引く
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public Point GetPositionFromViewBox(MouseEventArgs arg)
        {
            return arg.GetPosition(mainViewBox);
        }

        /// <summary>
        /// 複数ファイルの名前変更
        /// </summary>
        public FileRenameWindowViewModel RenameMultipleFiles(FileNameCheckFunc checkFunc)
        {
            var window = new FileRenameWindow()
            {
                Owner = this,
                CheckFunc = checkFunc
            };
            window.ShowDialog();

            var vm = window.DataContext as FileRenameWindowViewModel;
            return vm;
        }

        /// <summary>
        /// DataGridを指定アイテムへスクロールする
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ScrollToItem(FileItemViewModel item)
        {
            if (item != null)
            {
                fileListDataGrid.ScrollIntoView(item);
            }
            return true;
        }

        /// <summary>
        /// データグリッドのカレント（枠のある）セルを設定する
        /// </summary>
        /// <param name="item"></param>
        public void SetCurrentCell(FileItemViewModel item)
        {
            fileListDataGrid.CurrentItem = item;
        }

        /// <summary>
        /// 色選択ダイアログを表示する
        /// </summary>
        /// <returns></returns>
        public Color? ShowColorDialog()
        {
            System.Windows.Forms.ColorDialog dlg = new System.Windows.Forms.ColorDialog();
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return new Color()
                {
                    A = dlg.Color.A,
                    R = dlg.Color.R,
                    G = dlg.Color.G,
                    B = dlg.Color.B
                };
            }
            return null;
        }

        /// <summary>
        /// テキスト入力ウィンドウを表示する
        /// </summary>
        /// <returns></returns>
        public TextInputWindowViewModel ShowTextInputWindow()
        {
            TextInputWindow window = new TextInputWindow();
            window.ShowDialog();
            TextInputWindowViewModel vm = (TextInputWindowViewModel)window.DataContext;

            if (vm.IsOK && vm.Text != null)
            {
                return vm;
            }
            return null;
        }
    }
}
