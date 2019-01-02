using Prism.Commands;
using Prism.Mvvm;
using QuickEvidence.Views;

namespace QuickEvidence.ViewModels
{
    public class TextInputWindowViewModel : BindableBase
    {
        public TextInputWindow CloseIF { get; internal set; }
        public bool IsOK = false;

        public TextInputWindowViewModel()
        {

        }

        /// <summary>
        /// テキスト
        /// </summary>
        private string _text;
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        /// <summary>
        /// OKボタン
        /// </summary>
        private DelegateCommand _okCommand;
        public DelegateCommand OKCommand =>
            _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOKCommand));

        void ExecuteOKCommand()
        {
            IsOK = true;
            CloseIF.Close();
        }

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ?? (_cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        void ExecuteCancelCommand()
        {
            IsOK = false;
            CloseIF.Close();
        }


    }
}
