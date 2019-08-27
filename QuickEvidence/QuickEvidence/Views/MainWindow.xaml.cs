﻿using QuickEvidence.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QuickEvidence.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, IGetPosition, IColorDialog, ITextInputWindow, IScrollDataGrid, INavigation
    {
        public MainWindow()
        {
            InitializeComponent();

            MainWindowViewModel vm = (MainWindowViewModel)DataContext;
            vm.GetPositionIF = this;
            vm.ColorDialogIF = this;
            vm.TextInputWindowIF = this;
            vm.ScrollDataGridIF = this;
            vm.NavigationIF = this;
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
    }
}
