using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scrivener.Helpers
{
    public class MvvmTextEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
    {
       public  MvvmTextEditor() : base()
        {
            //Allows for recieving two different message types from VM
            // 2 type may not be necessary if all test control is handeled in this custom control
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(this, "insert", (action) => InsertQI(action));
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(this, "append", (action) => AppendQI(action));
            
            //Necessary for spell check system.
            this.TextArea.TextView.LineTransformers.Add(new SpellingErrorColorizer());
            this.Loaded += MvvmTextEditor_Loaded;
        }

       void MvvmTextEditor_Loaded(object sender, RoutedEventArgs e)
       {
           LoadContextStandards();
       }

        protected override void OnTextChanged(EventArgs e)
        {          
            RaisePropertyChanged("Text");
            
            if (ReturnFocus)
            {
                Application.Current.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate
                {
                    Keyboard.Focus(this);
                });
            }
            base.OnTextChanged(e);
        }

        public bool ReturnFocus
        {
            get { return (bool)GetValue(ReturnFocusProperty); }
            set { SetValue(ReturnFocusProperty, value); }
        }
        public static readonly DependencyProperty ReturnFocusProperty =
            DependencyProperty.Register("ReturnFocus", typeof(bool), typeof(MvvmTextEditor),
            new PropertyMetadata(false));



        private void InsertQI(string qi)
        {
            this.Document.Insert(this.TextArea.Caret.Offset, qi);
        }

        private void AppendQI(string qi)
        {
            this.AppendText(qi);
            //this.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Visible;
            this.ScrollToEnd(); //Scrolls window but does not set caret.
            this.CaretOffset = this.Text.Length;
        }

        private void LoadContextStandards()
        {


            
            this.ContextMenu = new ContextMenu();
            //Common Edit MenuItems.

            this.ContextMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Undo });
            this.ContextMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Redo });

            this.ContextMenu.Items.Add(new Separator());

            //Cut
            MenuItem cutMenuItem = new MenuItem();
            cutMenuItem.Command = ApplicationCommands.Cut;
            this.ContextMenu.Items.Add(cutMenuItem);
            //Copy
            MenuItem copyMenuItem = new MenuItem();
            copyMenuItem.Command = ApplicationCommands.Copy;
            this.ContextMenu.Items.Add(copyMenuItem);

            //Paste
            MenuItem pasteMenuItem = new MenuItem();
            pasteMenuItem.Command = ApplicationCommands.Paste;
            this.ContextMenu.Items.Add(pasteMenuItem);

            this.ContextMenu.Items.Add(new Separator());

            //Delete
            MenuItem deleteMenuItem = new MenuItem();
            deleteMenuItem.Command = ApplicationCommands.Delete;
            this.ContextMenu.Items.Add(deleteMenuItem);

            this.ContextMenu.Items.Add(new Separator());

            //Select All
            MenuItem selectAllMenuItem = new MenuItem();
            selectAllMenuItem.Command = ApplicationCommands.SelectAll;
            this.ContextMenu.Items.Add(selectAllMenuItem);

        }



        /// <summary>
        /// Raises a property changed event
        /// </summary>
        /// <param name="property">The name of the property that updates</param>
        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}
