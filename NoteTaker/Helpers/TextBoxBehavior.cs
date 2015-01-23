using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scrivener.Helpers
{
    //public class TextBoxBehaviour
    //{
    //    #region ScrollOnTextChange
    //    static readonly Dictionary<TextBox, Capture> _associations = new Dictionary<TextBox, Capture>();

    //    public static bool GetScrollOnTextChanged(DependencyObject dependencyObject)
    //    {
    //        return (bool)dependencyObject.GetValue(ScrollOnTextChangedProperty);
    //    }

    //    public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value)
    //    {
    //        dependencyObject.SetValue(ScrollOnTextChangedProperty, value);
    //    }

    //    public static readonly DependencyProperty ScrollOnTextChangedProperty =
    //        DependencyProperty.RegisterAttached("ScrollOnTextChange", typeof(bool), typeof(TextBoxBehaviour), new UIPropertyMetadata(false, OnScrollOnTextChange));

    //    static void OnScrollOnTextChange(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    //    {
    //        var textBox = dependencyObject as TextBox;
    //        if (textBox == null)
    //        {
    //            return;
    //        }
    //        bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
    //        if (newValue == oldValue)
    //        {
    //            return;
    //        }
    //        if (newValue)
    //        {
    //            textBox.Loaded += TextBoxLoaded;
    //            textBox.Unloaded += TextBoxUnloaded;
    //        }
    //        else
    //        {
    //            textBox.Loaded -= TextBoxLoaded;
    //            textBox.Unloaded -= TextBoxUnloaded;
    //            if (_associations.ContainsKey(textBox))
    //            {
    //                _associations[textBox].Dispose();
    //            }
    //        }
    //    }

    //    static void TextBoxUnloaded(object sender, RoutedEventArgs routedEventArgs)
    //    {
    //        var textBox = (TextBox)sender;
    //        _associations[textBox].Dispose();
    //        textBox.Unloaded -= TextBoxUnloaded;
    //    }

    //    static void TextBoxLoaded(object sender, RoutedEventArgs routedEventArgs)
    //    {
    //        var textBox = (TextBox)sender;
    //        textBox.Loaded -= TextBoxLoaded;
    //        _associations[textBox] = new Capture(textBox);
    //    }

    //    class Capture : IDisposable
    //    {
    //        private TextBox TextBox { get; set; }

    //        public Capture(TextBox textBox)
    //        {
    //            TextBox = textBox;
    //            TextBox.TextChanged += OnTextBoxOnTextChanged;
    //        }

    //        private void OnTextBoxOnTextChanged(object sender, TextChangedEventArgs args)
    //        {
    //            TextBox.ScrollToEnd();
    //        }

    //        public void Dispose()
    //        {
    //            TextBox.TextChanged -= OnTextBoxOnTextChanged;
    //        }
    //    }

    //    #endregion

    //    //prevents cusror from moving in a textbox on textchange
    //    #region NonIntrusiveText
    //    public static string GetNonIntrusiveText(DependencyObject obj)
    //    {
    //        return (string)obj.GetValue(NonIntrusiveTextProperty);
    //    }
    //    public static void SetNonIntrusiveText(DependencyObject obj, string value)
    //    {
    //        obj.SetValue(NonIntrusiveTextProperty, value);
    //    }
    //    public static readonly DependencyProperty NonIntrusiveTextProperty =
    //    DependencyProperty.RegisterAttached(
    //                    "NonIntrusiveText",
    //                    typeof(string),
    //                    typeof(TextBoxBehaviour),
    //    new FrameworkPropertyMetadata(
    //                        null,
    //                        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
    //                        NonIntrusiveTextChanged));
    //    public static void NonIntrusiveTextChanged(
    //                 object sender,
    //                 DependencyPropertyChangedEventArgs e)
    //    {
    //        var textBox = sender as TextBox;
    //        if (textBox == null) return;
    //        var caretIndex = textBox.CaretIndex;
    //        var selectionStart = textBox.SelectionStart;
    //        var selectionLength = textBox.SelectionLength;
    //        textBox.Text = (string)e.NewValue;
    //        textBox.CaretIndex = caretIndex;
    //        textBox.SelectionStart = selectionStart;
    //        textBox.SelectionLength = selectionLength;
    //    }
    //    #endregion
    //}      
}
