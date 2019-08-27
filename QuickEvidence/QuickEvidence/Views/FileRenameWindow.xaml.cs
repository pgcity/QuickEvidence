using QuickEvidence.ViewModels;
using System.Windows;

namespace QuickEvidence.Views
{
    /// <summary>
    /// Interaction logic for FileRenameWindow.xaml
    /// </summary>
    public partial class FileRenameWindow : Window, IClose
    {
        public FileRenameWindow()
        {
            InitializeComponent();

            var vm = DataContext as FileRenameWindowViewModel;

            vm.CloseIF = this;
        }

        public FileNameCheckFunc CheckFunc {
            get
            {
                var vm = DataContext as FileRenameWindowViewModel;
                return vm.CheckFunc;
            }
            set
            {
                var vm = DataContext as FileRenameWindowViewModel;
                vm.CheckFunc = value;
            }
        }
    }
}
