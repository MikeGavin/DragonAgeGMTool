using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Scrivener.Helpers
{
    // http://stackoverflow.com/a/18969007
    
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
                //AssociatedObject.KeyDown += AssociatedObject_KeyDown;
                AssociatedObject.PreviewKeyDown += AssociatedObject_KeyDown;
                AssociatedObject.Loaded += AssociatedObject_Loaded;
                AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseDown;
                //GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(this, "ProcessQI", (action) => ProcessQI(action));
            }
        }

        void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
                        var textEditor = sender as TextEditor;
                        if (textEditor != null)
                        {
                            if (textEditor.Document != null)
                            {
                                
                                CaretPosition = textEditor.CaretOffset;
                                
                            }
                        }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
                //AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                AssociatedObject.PreviewKeyDown -= AssociatedObject_KeyDown;
                AssociatedObject.Loaded -= AssociatedObject_Loaded;
                AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_MouseDown;
                
            }
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            LoadContextStandards(sender);
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            var textEditor = sender as TextEditor;
            if (textEditor != null)
            {
                if (textEditor.Document != null)
                {
                    var caretOffset = textEditor.CaretOffset;
                    GiveMeTheText = textEditor.Document.Text;                    
                    textEditor.CaretOffset = caretOffset;
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

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            var textEditor = sender as TextEditor;
            if (textEditor != null)
            {
                if (Properties.Settings.Default.DashinNotes == true)
                {
                    if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        var insertlinebreak = System.Environment.NewLine;
                        var selectionIndex2 = textEditor.SelectionStart;
                        textEditor.Text = textEditor.Text.Insert(selectionIndex2, insertlinebreak);
                        textEditor.SelectionStart = selectionIndex2 + insertlinebreak.Length;
                    }
                    else if (e.Key == Key.Enter)
                    {
                        e.Handled = true;
                        var insertText = System.Environment.NewLine + "- ";
                        var selectionIndex = textEditor.SelectionStart;
                        textEditor.Text = textEditor.Text.Insert(selectionIndex, insertText);
                        textEditor.SelectionStart = selectionIndex + insertText.Length;
                    }
                }
                else if (Properties.Settings.Default.DashinNotes == false)
                {
                    if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        e.Handled = true;
                        var insertText = System.Environment.NewLine + "- ";
                        var selectionIndex = textEditor.SelectionStart;
                        textEditor.Text = textEditor.Text.Insert(selectionIndex, insertText);
                        textEditor.SelectionStart = selectionIndex + insertText.Length;
                    }
                    else if (e.Key == Key.Enter)
                    {

                    }
                }
            }
        }

        public void LoadContextStandards(object sender)
        {
            var textEditor = sender as TextEditor;
            if (textEditor != null)
            {
                textEditor.ContextMenu = new ContextMenu();
                //Common Edit MenuItems.

                textEditor.ContextMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Undo });  //This is a cleaner way to do the below, change the rest when I have time.
                textEditor.ContextMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Redo });

                textEditor.ContextMenu.Items.Add(new Separator());

                //Cut
                MenuItem cutMenuItem = new MenuItem();
                cutMenuItem.Command = ApplicationCommands.Cut;
                textEditor.ContextMenu.Items.Add(cutMenuItem);
                //Copy
                MenuItem copyMenuItem = new MenuItem();
                copyMenuItem.Command = ApplicationCommands.Copy;
                textEditor.ContextMenu.Items.Add(copyMenuItem);

                //Paste
                MenuItem pasteMenuItem = new MenuItem();
                pasteMenuItem.Command = ApplicationCommands.Paste;
                textEditor.ContextMenu.Items.Add(pasteMenuItem);

                textEditor.ContextMenu.Items.Add(new Separator());

                //Delete
                MenuItem deleteMenuItem = new MenuItem();
                deleteMenuItem.Command = ApplicationCommands.Delete;
                textEditor.ContextMenu.Items.Add(deleteMenuItem);

                textEditor.ContextMenu.Items.Add(new Separator());

                //Select All
                MenuItem selectAllMenuItem = new MenuItem();
                selectAllMenuItem.Command = ApplicationCommands.SelectAll;
                textEditor.ContextMenu.Items.Add(selectAllMenuItem);

            }
        }

        public static readonly DependencyProperty GiveMeTheTextProperty =
         DependencyProperty.Register("GiveMeTheText", typeof(string), typeof(AvalonEditBehaviour),
         new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        public string GiveMeTheText
        {
            get { return (string)GetValue(GiveMeTheTextProperty); }
            set { SetValue(GiveMeTheTextProperty, value); }
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var behavior = dependencyObject as AvalonEditBehaviour;
            if (behavior.AssociatedObject != null)
            {
                var editor = behavior.AssociatedObject as TextEditor;

                if (editor.Document != null && args.NewValue != null)
                {            
                    //var caretOffset = editor.CaretOffset;
                    editor.Document.Text = args.NewValue.ToString();
                    //editor.CaretOffset = caretOffset;
                }
            }
        }


        public static readonly DependencyProperty CaretPositionProperity =
            DependencyProperty.Register("CaretPosition", typeof(int), typeof(AvalonEditBehaviour),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback2));

        public int CaretPosition
        {
            get { return (int)GetValue(CaretPositionProperity); }
            set { SetValue(CaretPositionProperity, value); }
        }

        private static void PropertyChangedCallback2(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var behavior = dependencyObject as AvalonEditBehaviour;
            if (behavior.AssociatedObject != null)
            {
                var editor = behavior.AssociatedObject as TextEditor;

                if (editor.Document != null && args.NewValue != null && (int)args.NewValue <= editor.CaretOffset)
                {
                    editor.CaretOffset = (int)args.NewValue;                    
                }
                else
                {
                    editor.ScrollToEnd();
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

        //private void ProcessQI(string qi)
        //{
        //    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        //    {
        //        var orgCaret = AssociatedObject.TextArea.Caret.Offset;
        //        AssociatedObject.Document.Insert(AssociatedObject.TextArea.Caret.Offset, qi);
        //        AssociatedObject.TextArea.Caret.Offset = orgCaret + qi.Length;               
        //    }
        //    else
        //    {
        //        AssociatedObject.AppendText(Environment.NewLine + qi);
        //        AssociatedObject.ScrollToEnd();
        //        AssociatedObject.CaretOffset = AssociatedObject.Text.Length;
        //    }
        //}
    }
}
