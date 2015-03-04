using System;
using System.Windows.Controls;

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

        private void NumberRegex(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Highlight(object sender, System.Windows.RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }
    }
}
