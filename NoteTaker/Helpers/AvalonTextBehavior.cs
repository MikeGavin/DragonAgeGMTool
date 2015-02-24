using ICSharpCode.AvalonEdit;
using Scrivener.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

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
                AssociatedObject.PreviewKeyDown += AssociatedObject_KeyDown;
                AssociatedObject.Loaded += AssociatedObject_Loaded;
                AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseDown;
                //allows mainVM to talk to this object.
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(this, "ProcessQI", (action) => ProcessQI(action));
                //Adds ctrl-f function, looks terrible though
                ICSharpCode.AvalonEdit.Search.SearchPanel.Install(AssociatedObject.TextArea);
                //gets Man apps current color to define link color so it is easier to read.
                AssociatedObject.TextArea.TextView.LinkTextForegroundBrush = (Brush)Application.Current.Resources["AccentColorBrush2"];
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

        void Caret_PositionChanged(object sender, EventArgs e)
        {
            var caret = sender as ICSharpCode.AvalonEdit.Editing.Caret;
            if (caret != null)
            {
                CaretPosition = caret.Offset;
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
                //AssociatedObject.TextArea.Caret.BringCaretToView();
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
                AssociatedObject.TextArea.Caret.BringCaretToView();
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
                    // without this if statement the following line will make the drag and drop move copy rather than move. 
                    // However removing the code causes the VM to be unable to update the binding.
                    if (args.NewValue.ToString() != editor.Document.Text) 
                    {
                        editor.Document.Text = args.NewValue.ToString(); 
                    }
                    
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

        private string DashCheck(string qn)
        {
            if (Properties.Settings.Default.DashinNotes)
            {
                return "- " + qn;
            }
            else
            {
                return qn;
            }
        }

        private void ProcessQI(string qn)
        {
            if (AssociatedObject.Text.Length < 2)
            {
                AssociatedObject.AppendText(DashCheck(qn));
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var orgCaret = AssociatedObject.TextArea.Caret.Offset;
                if (AssociatedObject.Text[AssociatedObject.CaretOffset - 1] == ' ')
                {
                    AssociatedObject.Document.Insert(AssociatedObject.TextArea.Caret.Offset, qn);

                }
                else
                {
                    AssociatedObject.Document.Insert(AssociatedObject.TextArea.Caret.Offset, " " + qn);                    
                }

                AssociatedObject.TextArea.Caret.Offset = orgCaret + qn.Length;                
            }
            else
            {
                var substring = AssociatedObject.Text.Substring(AssociatedObject.Text.Length - 2, 2);
                if (substring == "- ")
                {
                    AssociatedObject.AppendText(qn);
                }
                else if (Environment.NewLine == substring)
                {
                    AssociatedObject.AppendText(DashCheck(qn));
                }
                else
                {
                    AssociatedObject.AppendText(Environment.NewLine + DashCheck(qn));
                  
                }
                AssociatedObject.ScrollToEnd();                
                AssociatedObject.CaretOffset = AssociatedObject.Text.Length;
            }
        }
    }
}
