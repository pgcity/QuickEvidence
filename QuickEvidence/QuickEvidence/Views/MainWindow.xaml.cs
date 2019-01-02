using QuickEvidence.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace QuickEvidence.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, IGetPosition
    {
        public MainWindow()
        {
            InitializeComponent();

            MainWindowViewModel vm = (MainWindowViewModel)DataContext;
            vm.GetPositionIF = this;
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
    }
}
