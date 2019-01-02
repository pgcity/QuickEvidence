using QuickEvidence.ViewModels;
using System.Windows;

namespace QuickEvidence.Views
{
    /// <summary>
    /// Interaction logic for TextInputWindow.xaml
    /// </summary>
    public partial class TextInputWindow : Window, IClose
    {
        public TextInputWindow()
        {
            InitializeComponent();

            TextInputWindowViewModel vm = (TextInputWindowViewModel)DataContext;
            vm.CloseIF = this;
        }
    }
}
