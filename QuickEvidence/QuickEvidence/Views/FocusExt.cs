using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickEvidence.Views
{
    public static class FocusExt
    {


        public static string GetFocusedName(DependencyObject obj)
        {
            return (string)obj.GetValue(FocusedNameProperty);
        }

        public static void SetFocusedName(DependencyObject obj, int value)
        {
            obj.SetValue(FocusedNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for FocusedName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusedNameProperty =
            DependencyProperty.RegisterAttached("FocusedName", typeof(string), typeof(FocusExt), new PropertyMetadata("", OnFocusedNameChanged));

        private static void OnFocusedNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uiControl = d as Control;
            if(uiControl.Name == (string)e.NewValue)
            {
                uiControl.Focus();
                uiControl.GotFocus -= UIControl_GotFocus;
                uiControl.LostFocus += UIControl_LostFocus;
            }
            else
            {
                uiControl.GotFocus += UIControl_GotFocus;
                uiControl.LostFocus -= UIControl_LostFocus;
            }
        }

        private static void UIControl_LostFocus(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;

            //フォーカス消失したのが自コントロール
            if(control.GetValue(FocusedNameProperty) as string == control.Name)
            {
                control.SetValue(FocusedNameProperty, null);
            }
        }

        private static void UIControl_GotFocus(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            control.SetValue(FocusedNameProperty, control.Name);
        }
    }
}
