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

        public Point GetPosition(MouseEventArgs arg)
        {
            return arg.GetPosition(mainScrollViewer);
        }
    }
}
