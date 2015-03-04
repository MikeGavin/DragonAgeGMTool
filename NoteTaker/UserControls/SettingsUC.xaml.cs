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
                    Properties.Settings.Default.Accent = 0;
                }               
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

    }
}
