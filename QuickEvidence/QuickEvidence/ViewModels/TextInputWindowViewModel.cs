using Prism.Commands;
using Prism.Mvvm;
using QuickEvidence.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace QuickEvidence.ViewModels
{
    public class TextInputWindowViewModel : BindableBase
    {
        public TextInputWindow CloseIF { get; internal set; }
        public bool IsOK = false;

        public TextInputWindowViewModel()
        {
            FontFamily = (from font in Fonts.SystemFontFamilies where font.Source == Properties.Settings.Default.FontFamily select font).FirstOrDefault();
            FontSize = Properties.Settings.Default.FontSize;
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
        /// フォントリスト
        /// </summary>
        private ObservableCollection<FontFamily> _fontList = new ObservableCollection<FontFamily>(Fonts.SystemFontFamilies);
        public ObservableCollection<FontFamily> FontList
        {
            get { return _fontList; }
            set { SetProperty(ref _fontList, value); }
        }

        /// <summary>
        /// フォント
        /// </summary>
        private FontFamily _fontFamily;
        public FontFamily FontFamily
        {
            get { return _fontFamily; }
            set { SetProperty(ref _fontFamily, value); }
        }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        private int _fontSize;
        public int FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, value); }
        }

        /// <summary>
        /// OKボタン
        /// </summary>
        private DelegateCommand _okCommand;
        public DelegateCommand OKCommand =>
            _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOKCommand));

        void ExecuteOKCommand()
        {
            Properties.Settings.Default.FontFamily = FontFamily.Source;
            Properties.Settings.Default.FontSize = FontSize;
            Properties.Settings.Default.Save();

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
