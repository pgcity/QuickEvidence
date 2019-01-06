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
    public partial class MainWindow : Window, IGetPosition, IColorDialog, ITextInputWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            MainWindowViewModel vm = (MainWindowViewModel)DataContext;
            vm.GetPositionIF = this;
            vm.ColorDialogIF = this;
            vm.TextInputWindowIF = this;
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
        public string ShowTextInputWindow()
        {
            TextInputWindow window = new TextInputWindow();
            window.ShowDialog();
            TextInputWindowViewModel vm = (TextInputWindowViewModel)window.DataContext;

            if (vm.IsOK)
            {
                return vm.Text;
            }
            return null;
        }

        /// <summary>
        /// ファイル一覧の選択変更
        /// 変更先アイテムが見えるようスクロールする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var listView = (System.Windows.Controls.DataGrid)sender;

            MainWindowViewModel vm = (MainWindowViewModel)DataContext;
            if(vm.SelectedFile != null)
            {
                listView.ScrollIntoView(vm.SelectedFile);
            }
        }

        /// <summary>
        /// ファイルダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenHyperlink_Clicked(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel vm = (MainWindowViewModel)DataContext;
            if (vm.SelectedFile != null)
            {
                System.Diagnostics.Process.Start(vm.SelectedFile.FullPath);
            }
        }
    }
}
