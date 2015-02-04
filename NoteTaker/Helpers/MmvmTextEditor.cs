//using ICSharpCode.AvalonEdit;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;

//namespace Scrivener.Helpers
//{
//    /// <summary>
//    /// Class that inherits from the AvalonEdit TextEditor control to 
//    /// enable MVVM interaction. 
//    /// </summary>
//    public class MvvmTextEditor : TextEditor, INotifyPropertyChanged
//    {
//        #region RaiseProperityChange
//        /// <summary>
//        /// Implement the INotifyPropertyChanged event handler.
//        /// </summary>
//        public event PropertyChangedEventHandler PropertyChanged;
//        public void RaisePropertyChanged([CallerMemberName] string caller = null)
//        {
//            var handler = PropertyChanged;
//            if (handler != null)
//                PropertyChanged(this, new PropertyChangedEventArgs(caller));
//        }

//        #endregion

//        /// <summary>
//        /// Default constructor to set up event handlers.
//        /// </summary>
//        public MvvmTextEditor()
//        {
//            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(this, "ProcessQI", (action) => ProcessQI(action));
//            TextArea.SelectionChanged += TextArea_SelectionChanged;
//            Loaded += MvvmTextEditor_Loaded;
//        }

//        /// <summary>
//        /// Event handler to load default content menu on load.
//        /// </summary>
//        void MvvmTextEditor_Loaded(object sender, RoutedEventArgs e)
//        {
//            LoadContextStandards(sender);
//        }

//        /// <summary>
//        /// Event handler to update properties based upon the selection changed event.
//        /// </summary>
//        void TextArea_SelectionChanged(object sender, EventArgs e)
//        {
//            this.SelectionStart = SelectionStart;
//            this.SelectionLength = SelectionLength;
//        }

//        #region Text
//        /// <summary>
//        /// Dependancy property for the editor text property binding.
//        /// </summary>
//        public static readonly DependencyProperty TextProperty =
//             DependencyProperty.Register("Text", typeof(string), typeof(MvvmTextEditor),
//             new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) =>
//             {
//                 MvvmTextEditor target = (MvvmTextEditor)obj;
//                 target.Text = (string)args.NewValue;
//             }));

//        /// <summary>
//        /// Provide access to the Text.
//        /// </summary>
//        public new string Text
//        {
//            get { return (string)GetValue(TextProperty); }
//            set { SetValue(TextProperty, value); }
//        }

//        /// <summary>
//        /// Return the current text length.
//        /// </summary>
//        public int Length
//        {
//            get { return base.Text.Length; }
//        }

//        /// <summary>
//        /// Override of OnTextChanged event.
//        /// </summary>
//        protected override void OnTextChanged(EventArgs e)
//        {
//            RaisePropertyChanged("Text");
//            if (this.Document != null)
//            {
//                if (this.Document != null)
//                {
//                    GiveMeTheText = this.Document.Text;
//                }
//            }
//            base.OnTextChanged(e);
//            if (ReturnFocus)
//            {
//                Application.Current.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate
//                {
//                    Keyboard.Focus(this.TextArea);
//                });
//            }
//        }

//        public static readonly DependencyProperty GiveMeTheTextProperty =
// DependencyProperty.Register("GiveMeTheText", typeof(string), typeof(MvvmTextEditor),
// new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

//        public string GiveMeTheText
//        {
//            get { return (string)GetValue(GiveMeTheTextProperty); }
//            set { SetValue(GiveMeTheTextProperty, value); }
//        }

//        private static void PropertyChangedCallback(
//            DependencyObject dependencyObject,
//            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
//        {
//            var behavior = dependencyObject as MvvmTextEditor;
//            if (behavior != null)
//            {
//                var editor = behavior as TextEditor;

//                if (editor.Document != null && dependencyPropertyChangedEventArgs.NewValue != null)
//                {
//                    var caretOffset = editor.CaretOffset;
//                    editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();
//                    if (caretOffset <= editor.Text.Length)
//                        editor.CaretOffset = caretOffset;
//                    else
//                        editor.CaretOffset = editor.Text.Length;
//                }
//            }
//        }
        
        
//        #endregion // Text.

//        #region Caret Offset
//        /// <summary>
//        /// DependencyProperty for the TextEditorCaretOffset binding. 
//        /// </summary>
//        public static DependencyProperty CaretOffsetProperty =
//            DependencyProperty.Register("CaretOffset", typeof(int), typeof(MvvmTextEditor),
//            new PropertyMetadata((obj, args) =>
//            {
//                MvvmTextEditor target = (MvvmTextEditor)obj;
//                target.CaretOffset = (int)args.NewValue;
//            }));

//        /// <summary>
//        /// Provide access to the CaretOffset.
//        /// </summary>
//        public new int CaretOffset
//        {
//            get { return base.CaretOffset; }
//            set { base.CaretOffset = value; }
//        }
//        #endregion // Caret Offset.

//        #region Selection
//        /// <summary>
//        /// DependencyProperty for the TextEditor SelectionLength property. 
//        /// </summary>
//        public static readonly DependencyProperty SelectionLengthProperty =
//             DependencyProperty.Register("SelectionLength", typeof(int), typeof(MvvmTextEditor),
//             new PropertyMetadata((obj, args) =>
//             {
//                 MvvmTextEditor target = (MvvmTextEditor)obj;
//                 target.SelectionLength = (int)args.NewValue;
//             }));

//        /// <summary>
//        /// Access to the SelectionLength property.
//        /// </summary>
//        public new int SelectionLength
//        {
//            get { return base.SelectionLength; }
//            set { SetValue(SelectionLengthProperty, value); }
//        }

//        /// <summary>
//        /// DependencyProperty for the TextEditor SelectionStart property. 
//        /// </summary>
//        public static readonly DependencyProperty SelectionStartProperty =
//             DependencyProperty.Register("SelectionStart", typeof(int), typeof(MvvmTextEditor),
//             new PropertyMetadata((obj, args) =>
//             {
//                 MvvmTextEditor target = (MvvmTextEditor)obj;
//                 target.SelectionStart = (int)args.NewValue;
//             }));

//        /// <summary>
//        /// Access to the SelectionStart property.
//        /// </summary>
//        public new int SelectionStart
//        {
//            get { return base.SelectionStart; }
//            set { SetValue(SelectionStartProperty, value); }
//        }
//        #endregion // Selection.

//        #region Return Focus
//        /// <summary>
//        /// DependencyProperty for the TextEditor returnfocus property. 
//        /// </summary>
//        public static readonly DependencyProperty ReturnFocusProperty =
//            DependencyProperty.Register("ReturnFocus", typeof(bool), typeof(AvalonEditBehaviour),
//            new PropertyMetadata(false));
//        public bool ReturnFocus
//        {
//            get { return (bool)GetValue(ReturnFocusProperty); }
//            set { SetValue(ReturnFocusProperty, value); }
//        }
//        #endregion

//        private void ProcessQI(string qi)
//        {
//            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
//            {
//                var orgCaret = base.TextArea.Caret.Offset;
//                base.Document.Insert(base.TextArea.Caret.Offset, qi);
//                base.TextArea.Caret.Offset = orgCaret + qi.Length;
//            }
//            else
//            {
//                base.AppendText(qi);
//                base.ScrollToEnd();
//                base.CaretOffset = base.Text.Length;
//            }
//        }

//        public void LoadContextStandards(object sender)
//        {
//            var textEditor = sender as TextEditor;
//            if (textEditor != null)
//            {
//                textEditor.ContextMenu = new ContextMenu();
//                //Common Edit MenuItems.

//                textEditor.ContextMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Undo });  //This is a cleaner way to do the below, change the rest when I have time.
//                textEditor.ContextMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Redo });

//                textEditor.ContextMenu.Items.Add(new Separator());

//                //Cut
//                MenuItem cutMenuItem = new MenuItem();
//                cutMenuItem.Command = ApplicationCommands.Cut;
//                textEditor.ContextMenu.Items.Add(cutMenuItem);
//                //Copy
//                MenuItem copyMenuItem = new MenuItem();
//                copyMenuItem.Command = ApplicationCommands.Copy;
//                textEditor.ContextMenu.Items.Add(copyMenuItem);

//                //Paste
//                MenuItem pasteMenuItem = new MenuItem();
//                pasteMenuItem.Command = ApplicationCommands.Paste;
//                textEditor.ContextMenu.Items.Add(pasteMenuItem);

//                textEditor.ContextMenu.Items.Add(new Separator());

//                //Delete
//                MenuItem deleteMenuItem = new MenuItem();
//                deleteMenuItem.Command = ApplicationCommands.Delete;
//                textEditor.ContextMenu.Items.Add(deleteMenuItem);

//                textEditor.ContextMenu.Items.Add(new Separator());

//                //Select All
//                MenuItem selectAllMenuItem = new MenuItem();
//                selectAllMenuItem.Command = ApplicationCommands.SelectAll;
//                textEditor.ContextMenu.Items.Add(selectAllMenuItem);

//            }
//        }
    
//    }

//}
