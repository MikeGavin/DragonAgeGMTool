using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Scrivener.Helpers
{
    // http://stackoverflow.com/a/18969007

    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty GiveMeTheTextProperty =
            DependencyProperty.Register("GiveMeTheText", typeof(string), typeof(AvalonEditBehaviour),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        public string GiveMeTheText
        {
            get { return (string)GetValue(GiveMeTheTextProperty); }
            set { SetValue(GiveMeTheTextProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            var textEditor = sender as TextEditor;
            if (textEditor != null)
            {
                if (textEditor.Document != null)
                {
                    GiveMeTheText = textEditor.Document.Text;
                }

                if (ReturnFocus)
                {
                    Application.Current.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate
                    {
                        Keyboard.Focus(textEditor);
                    });
                }
            }
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as AvalonEditBehaviour;
            if (behavior.AssociatedObject != null)
            {
                var editor = behavior.AssociatedObject as TextEditor;
                if (editor.Document != null && dependencyPropertyChangedEventArgs.NewValue != null)
                {
                    //var caretOffset = editor.CaretOffset;
                    editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();
                    //editor.CaretOffset = caretOffset;
                }
            }
        }


        public bool ReturnFocus
        {
            get { return (bool)GetValue(ReturnFocusProperty); }
            set { SetValue(ReturnFocusProperty, value); }
        }
        public static readonly DependencyProperty ReturnFocusProperty =
            DependencyProperty.Register("ReturnFocus", typeof(bool), typeof(AvalonEditBehaviour),
            new PropertyMetadata(false));


    }
}
