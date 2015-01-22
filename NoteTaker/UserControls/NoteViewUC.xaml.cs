using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Scrivener.UserControls
{
    /// <summary>
    /// Interaction logic for Note.xaml
    /// </summary>
    public partial class NoteViewUC : UserControl
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public NoteViewUC()
        {                       
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private void Noteareakeydown(object sender, KeyEventArgs e)
        {
            if (Properties.Settings.Default.DashinNotes == true)
            {
                if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    e.Handled = true;
                    var insertlinebreak = System.Environment.NewLine;
                    var selectionIndex2 = Notearea.SelectionStart;
                    Notearea.Text = Notearea.Text.Insert(selectionIndex2, insertlinebreak);
                    Notearea.SelectionStart = selectionIndex2 + insertlinebreak.Length;
                }
                else if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    var insertText = System.Environment.NewLine + "- ";
                    var selectionIndex = Notearea.SelectionStart;
                    Notearea.Text = Notearea.Text.Insert(selectionIndex, insertText);
                    Notearea.SelectionStart = selectionIndex + insertText.Length;
                }
            }
            else if (Properties.Settings.Default.DashinNotes == false)
            {
                if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    e.Handled = true;
                    var insertText = System.Environment.NewLine + "- ";
                    var selectionIndex = Notearea.SelectionStart;
                    Notearea.Text = Notearea.Text.Insert(selectionIndex, insertText);
                    Notearea.SelectionStart = selectionIndex + insertText.Length;
                }
                else if (e.Key == Key.Enter)
                {

                }
            }          
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {

            var oldIndex = Notearea.CaretIndex;
            Application.Current.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate
            {
                //Keyboard.Focus(Notearea);
                Notearea.Focus();
                //Notearea.CaretIndex = index;
            });
        }    
    }
}
