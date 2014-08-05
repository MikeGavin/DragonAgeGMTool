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
        public NoteViewUC()
        {
            InitializeComponent();
        }

        private void Noteareakeydown(object sender, KeyEventArgs e)
        {
            //if (Properties.Settings.Default.DashinNotes == true)
            //{
            //    var insertlinebreak = System.Environment.NewLine;
            //    var selectionIndex = Notearea.Document.ContentStart.DocumentStart;
            //    var insertText = System.Environment.NewLine + "- ";
            //    TextPointer caretPos = Notearea.CaretPosition;

            //    if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            //    {
            //        e.Handled = true;
            //        Notearea.Text = insertlinebreak;
            //    }
            //    else if (e.Key == Key.Enter)
            //    {
            //        e.Handled = true;
            //        if (Notearea.CaretPosition == Notearea.Document.ContentEnd)
            //        {
            //            Notearea.Text = Notearea.Text.Insert(caretPos, insertText);
            //        }
            //        else
            //        {
            //            Notearea.Text.Insert(Notearea.CaretPosition, insertText);
            //        }
            //    }
            //}
            //else if (Properties.Settings.Default.DashinNotes == false)
            //{

            //}







            //if (Properties.Settings.Default.DashinNotes == true)
            //{
            //    if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            //    {
            //        e.Handled = true;
            //        var insertlinebreak = System.Environment.NewLine;
            //        var selectionIndex2 = Notearea.SelectionStart;
            //        Notearea.Text = Notearea.Text.Insert(selectionIndex2, insertlinebreak);
            //        Notearea.SelectionStart = selectionIndex2 + insertlinebreak.Length;
            //    }
            //    else if (e.Key == Key.Enter)
            //    {
            //        e.Handled = true;
            //        var insertText = System.Environment.NewLine + "- ";
            //        var selectionIndex = Notearea.SelectionStart;
            //        Notearea.Text = Notearea.Text.Insert(selectionIndex, insertText);
            //        Notearea.SelectionStart = selectionIndex + insertText.Length;
            //    }
            //}
            //else if (Properties.Settings.Default.DashinNotes == false)
            //{

            //}
        }
    }
}
