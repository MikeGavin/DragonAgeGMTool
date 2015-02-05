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
using MahApps.Metro;

namespace Scrivener.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsUC.xaml
    /// </summary>
    public partial class SettingsUC : UserControl
    {
        public SettingsUC()
        {
            InitializeComponent();

                if (Scrivener.Properties.Settings.Default.Accent == -1)
                {
                    Properties.Settings.Default.Accent = 2;
                }

               //Theme Settings
                ThemeBox.Items.Add("Dark");
                ThemeBox.Items.Add("Light");
                if (Properties.Settings.Default.Theme == null)
                    Properties.Settings.Default.Theme = "Dark";
                ThemeBox.SelectedItem = Properties.Settings.Default.Theme;

                //Accent Settings
                var accents = MahApps.Metro.ThemeManager.Accents.ToList();
                foreach (var accent in accents) //Adds all accents to combobox.
                {
                    AccentBox.Items.Add(accent);
                }
        }

        private void Accent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var accent = (MahApps.Metro.Accent)AccentBox.SelectedItem;
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, ThemeManager.GetAppTheme(string.Format("Base{0}", ThemeBox.SelectedItem)));
        }

        private void Phoneregex(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CompileBF(object sender, RoutedEventArgs e)
        {
            String replaceitems = "Please replace: ";
            String bfdate = "";
            String bftime = "";
            ComboBoxItem starttime = StartTime.SelectedItem as ComboBoxItem;
            ComboBoxItem stoptime = StopTime.SelectedItem as ComboBoxItem;
            String bfnumber = "";
            String bfaddy = "";

            String noitem = "";
            String nodate = "";
            String nostarttime = "";
            String nostoptime = "";
            String noaddy = "";
            String nonumber = "";

                        
            foreach (ListBoxItem selecteditems in bflistbox.SelectedItems)
            {
                replaceitems += selecteditems.Content.ToString() + ", " + Environment.NewLine;
            }
            int i = replaceitems.LastIndexOf(",");
            if (i != -1)
            {
                replaceitems = replaceitems.Remove(i);
            }

            bool ideliver = false;

            if (replaceitems.Contains("Tower") || replaceitems.Contains("Monitor") || replaceitems.Contains("Printer"))
            {
                ideliver = true;
            }

            if (BFDate.SelectedDate != null)
            {
                DateTime testdate = BFDate.SelectedDate.Value.Date;
                bfdate = "Scheduled for: " + testdate.ToString("dddd - MM/dd/yyyy") + Environment.NewLine;
            }
            

            if (starttime.Content.ToString() == "Start Time" && stoptime.Content.ToString() == "Stop Time")
            {
                bftime = " between " + starttime.Content.ToString() +" and " + stoptime.Content.ToString();
            }     
            
            if (BFNumber.Text != "")
            {
                bfnumber = "Best contact number: " + BFNumber.Text + Environment.NewLine;
            }

            if (BFAddy.Text != "")
            {
                bfaddy = BFAddy.Text + Environment.NewLine;
            }

            if (bflistbox.SelectedItems.Count == 0)
            { 
                noitem = "Please select an item(s) to be replaced" + Environment.NewLine + Environment.NewLine; 
            }
            else
            {
                noitem = "";
            }

            if (BFDate.SelectedDate == null && ideliver == true)
            {
                nodate = "Please select ''Scheduled for'' date" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                nodate = "";
            }
            if (starttime.Content.ToString() == "Start Time" && ideliver == true)
            {
                nostarttime = "Please select ''Start Time''" + Environment.NewLine + Environment.NewLine;
            }   
            else
            {
                nostarttime = "";
            }

            if (stoptime.Content.ToString() == "Stop Time" && ideliver == true)
            {
                nostoptime = "Please select ''Stop Time''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                nostoptime = "";
            }

            if (BFAddy.Text == "")
            {
                noaddy = "Please include ''Address''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                noaddy = "";
            }

            if (BFNumber.Text == "")
            {
                nonumber = "Please include ''Phone Number''" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                nonumber = "";
            }


            if (bflistbox.SelectedItems.Count == 0 || BFAddy.Text == "" || BFNumber.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noaddy + nonumber);
            }
            else if (ideliver == true && bfdate == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noaddy + nonumber);
            }
            else if (ideliver == true && starttime.Content.ToString() == "Start Time")
                {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noaddy + nonumber);
            }
            else if (ideliver == true && stoptime.Content.ToString() == "Stop Time")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noaddy + nonumber);
            }
            else if (ideliver == true && BFNumber.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noaddy + nonumber);
            }
            else if (ideliver == true && BFAddy.Text == "")
            {
                MessageBox.Show(noitem + nodate + nostarttime + nostoptime + noaddy + nonumber);
            }
            else
            {
                MessageBox.Show(replaceitems + bfdate + bftime + bfnumber + bfaddy);
            }
        }
    }
}
