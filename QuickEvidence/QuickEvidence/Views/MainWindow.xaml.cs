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
    public partial class MainWindow : Window, IGetPosition, IColorDialog
    {
        public MainWindow()
        {
            InitializeComponent();

            MainWindowViewModel vm = (MainWindowViewModel)DataContext;
            vm.GetPositionIF = this;
            vm.ColorDialogIF = this;
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
    }
}
