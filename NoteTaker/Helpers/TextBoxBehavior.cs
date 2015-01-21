using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Scrivener.Helpers
{
    public class TextBoxBehaviour
    {
        static readonly Dictionary<TextBox, Capture> _associations = new Dictionary<TextBox, Capture>();

        public static bool GetScrollOnTextChanged(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(ScrollOnTextAppendProperty);
        }

        public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(ScrollOnTextAppendProperty, value);
        }

        public static readonly DependencyProperty ScrollOnTextAppendProperty =
            DependencyProperty.RegisterAttached("ScrollOnTextAppend", typeof(bool), typeof(TextBoxBehaviour), new UIPropertyMetadata(false, OnScrollOnTextAppend));

        static void OnScrollOnTextAppend(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
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
                //Edited to only scroll only when the append is used.                 
                if (args.Changes.Any( (p) => p.Offset == 0))
                {
                    TextBox.ScrollToEnd();
                    TextBox.CaretIndex = TextBox.Text.Length;
                    //System.Windows.Input.Keyboard.Focus(TextBox);
                    //var test = TextBox.Focus();
                    //Notearea.Focus();
                    //Notearea.Focusable = false;
                    //FocusManager.SetFocusedElement(this, Notearea); 
                    //Keyboard.Focus(Notearea);
                    
                    //this is the only focus method I was able to get to work
                    Application.Current.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate
                    {
                        TextBox.Focus();
                    });


                }
                
            }

            public void Dispose()
            {
                TextBox.TextChanged -= OnTextBoxOnTextChanged;
            }
        }


        public static readonly DependencyProperty AlwaysScrollToEndProperty = DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), typeof(TextBoxBehaviour), new PropertyMetadata(false, AlwaysScrollToEndChanged));

        private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                TextBox tb = sender as TextBox;
                if (tb != null) {
                    bool alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;
                    if (alwaysScrollToEnd) {
                        tb.ScrollToEnd();
                        tb.TextChanged += TextChanged;
                    } else {
                        tb.TextChanged -= TextChanged;
                    }
                } else {
                    throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to TextBox instances.");
                }
            }

            public static bool GetAlwaysScrollToEnd(TextBox textBox)
            {
                if (textBox == null) {
                    throw new ArgumentNullException("textBox");
                }

                return (bool)textBox.GetValue(AlwaysScrollToEndProperty);
            }

            public static void SetAlwaysScrollToEnd(TextBox textBox, bool alwaysScrollToEnd)
            {
                if (textBox == null) {
                    throw new ArgumentNullException("textBox");
                }

                textBox.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
            }

            private static void TextChanged(object sender, TextChangedEventArgs e)
            {
                ((TextBox)sender).ScrollToEnd();
            }
            
        }
    
}
