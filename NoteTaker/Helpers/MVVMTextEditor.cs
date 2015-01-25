using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Scrivener.Helpers
{
    public class MvvmTextEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
    {
       public  MvvmTextEditor() : base()
        {
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(this, "insert", (action) => InsertQI(action));
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(this, "append", (action) => AppendQI(action));

            
        }

        /// <summary>
        /// A bindable Text property
        /// </summary>
        public new string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// The bindable text property dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MvvmTextEditor), new PropertyMetadata((obj, args) =>
            {
                var target = (MvvmTextEditor)obj;
                target.Text = (string)args.NewValue;
            }));

        protected override void OnTextChanged(EventArgs e)
        {
            RaisePropertyChanged("Text");
            base.OnTextChanged(e);
            if (ReturnFocus)
            {
                Application.Current.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate
                {
                    Keyboard.Focus(this);
                });
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
