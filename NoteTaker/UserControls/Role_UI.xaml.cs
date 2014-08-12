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
using System.Windows.Shapes;
using System.Diagnostics;

namespace Scrivener.UserControls
{
    /// <summary>
    /// Interaction logic for Role_UI.xaml
    /// </summary>
    public partial class Role_UI : Window
    {
        public Role_UI()
        {
            InitializeComponent();
            App.Current.MainWindow.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Role == (-1))
            {
                MessageBox.Show("Please pick a role.");
            }
            else
            {
                Properties.Settings.Default.Save();           
                    //var MWindow = new MainWindow();
                    //MWindow.ShowDialog();
                this.Close();
            }
        }
    }
}
