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
    /// Interaction logic for EditableTextBlockUC.xaml
    /// </summary>
    public partial class EditableTextBlockUC : UserControl
    {
        public EditableTextBlockUC()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
              typeof(EditableTextBlockUC), new UIPropertyMetadata());


        private void textBlock_MouseRightButtonUp(object sender
                                       , MouseButtonEventArgs e)
        {
            textBlock.Visibility = System.Windows.Visibility.Collapsed;
            editBox.Text = textBlock.Text;
            editBox.SelectAll();
            editBox.Visibility = Visibility.Visible;
            editBox.Height = textBlock.Height;
            editBox.Width = textBlock.Width;
            editBox.Focus();
        }

        private void editBox_LostFocus(object sender, RoutedEventArgs e)
        {
            textBlock.Text = editBox.Text;
            Properties.Settings.Default.UpdatedCharname = editBox.Text;
            textBlock.Visibility = System.Windows.Visibility.Visible;
            editBox.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void editBox_enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                textBlock.Text = editBox.Text;
                textBlock.Visibility = System.Windows.Visibility.Visible;
                editBox.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
