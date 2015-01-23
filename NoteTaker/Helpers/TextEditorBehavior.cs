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
    public class TextEditorBehaviour
    {
        #region ScrollOnTextChange
        static readonly Dictionary<TextBox, Capture> _associations = new Dictionary<TextBox, Capture>();

        public static bool GetScrollOnTextChanged(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(ScrollOnTextChangedProperty);
        }

        public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(ScrollOnTextChangedProperty, value);
        }

        public static readonly DependencyProperty ScrollOnTextChangedProperty =
            DependencyProperty.RegisterAttached("ScrollOnTextChange", typeof(bool), typeof(TextEditorBehaviour), new UIPropertyMetadata(false, OnScrollOnTextChange));

        static void OnScrollOnTextChange(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            if (textBox == null)
            {
                return;
            }
            bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
            if (newValue == oldValue)
            {
                return;
            }
            if (newValue)
            {
                textBox.Loaded += TextBoxLoaded;
                textBox.Unloaded += TextBoxUnloaded;
            }
            else
            {
                textBox.Loaded -= TextBoxLoaded;
                textBox.Unloaded -= TextBoxUnloaded;
                if (_associations.ContainsKey(textBox))
                {
                    _associations[textBox].Dispose();
                }
            }
        }

        static void TextBoxUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            _associations[textBox].Dispose();
            textBox.Unloaded -= TextBoxUnloaded;
        }

        static void TextBoxLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            textBox.Loaded -= TextBoxLoaded;
            _associations[textBox] = new Capture(textBox);
        }

        class Capture : IDisposable
        {
            private TextBox TextBox { get; set; }

            public Capture(TextBox textBox)
            {
                TextBox = textBox;
                TextBox.TextChanged += OnTextBoxOnTextChanged;
            }

            private void OnTextBoxOnTextChanged(object sender, TextChangedEventArgs args)
            {
                TextBox.ScrollToEnd();
            }

            public void Dispose()
            {
                TextBox.TextChanged -= OnTextBoxOnTextChanged;
            }
        }

        #endregion

        #region ReturnFocus
        public static string GetReturnFocus(DependencyObject obj)
        {
            return (string)obj.GetValue(ReturnFocusProperty);
        }
        public static void SetNonIntrusiveText(DependencyObject obj, string value)
        {
            obj.SetValue(ReturnFocusProperty, value);
        }
        public static readonly DependencyProperty ReturnFocusProperty =
        DependencyProperty.RegisterAttached(
                        "ReturnFocus",
                        typeof(string),
                        typeof(TextEditorBehaviour),
        new FrameworkPropertyMetadata(
                            null,
                            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                            OnTextChanged));
        public static void OnTextChanged(
                     object sender,
                     DependencyPropertyChangedEventArgs e)
        {
            var editor = (TextEditor)sender;

            Application.Current.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate
            {
                Keyboard.Focus(editor);
            });
        }
        #endregion
    }    
}
