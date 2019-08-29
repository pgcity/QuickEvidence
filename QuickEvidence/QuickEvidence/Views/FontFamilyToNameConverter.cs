using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace QuickEvidence.Views
{
    internal class FontFamilyToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var lang = (parameter as string != null) ? parameter as string : "ja-jp";
            var v = value as FontFamily;
            var currentLang = XmlLanguage.GetLanguage(lang);
            return v.FamilyNames.FirstOrDefault(o => o.Key == currentLang).Value ?? v.Source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
