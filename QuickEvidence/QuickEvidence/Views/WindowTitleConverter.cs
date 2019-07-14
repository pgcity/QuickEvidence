using QuickEvidence.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace QuickEvidence.Views
{
    /// <summary>
    /// 選択ファイル名からWindowタイトルに変換する
    /// </summary>
    public class WindowTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return parameter;
            }
            var fileItem = value as FileItemViewModel;
            return fileItem.FileName + " - " + parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
