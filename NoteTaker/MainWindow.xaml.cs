using System.Windows;
using Scrivener.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Windows.Input;
using System.Windows.Controls;
using System;
using MahApps.Metro;
using System.Linq;
using Scrivener.UserControls;


namespace Scrivener
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow

    {       
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //if (Properties.Settings.Default.Role == -1)
            //{
            //    this.Hide();
            //    var profile_window = new Role_UI();
            //    profile_window.Show();
            //}

                Closing += (s, e) => ViewModelLocator.Cleanup();

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



                //if (Properties.Settings.Default.Note_WorkSpace_Visibility == true)
                //{
                //    Properties.Settings.Default.Notespace_Test = System.Windows.Visibility.Visible.ToString();
                //    Tabs.Visibility = System.Windows.Visibility.Visible;
                //}
                //else
                //{
                //    Properties.Settings.Default.Notespace_Test = System.Windows.Visibility.Collapsed.ToString();
                //    Tabs.Visibility = System.Windows.Visibility.Collapsed;
                //}
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsFlyout.IsOpen == false)
            {
                SettingsFlyout.IsOpen = true;
            }
            else
            {
                SettingsFlyout.IsOpen = false;
            }

        }

        private void TabLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void SettingsFlyout_IsOpenChanged(object sender, EventArgs e)
        {
            //if (Properties.Settings.Default.Note_WorkSpace_Visibility == false)
            //{
            //    Properties.Settings.Default.QuickNotes_Visibility = false;
            //}
            Properties.Settings.Default.Save();
            
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
    }
}
