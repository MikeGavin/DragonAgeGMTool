using System.Windows;
using NoteTaker.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Windows.Input;
using System.Windows.Controls;
using System;

namespace NoteTaker
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
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Messenger.Default.Register<Helpers.MessageDialog>(
                this,
                async msg =>
                {
                    var metroWindow = (Application.Current.MainWindow as MetroWindow);
                    metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
                    await metroWindow.ShowMessageAsync(msg.Title, msg.Message, MessageDialogStyle.Affirmative, metroWindow.MetroDialogOptions);
                });
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsFlyout.IsOpen == false)
                SettingsFlyout.IsOpen = true;
            else
                SettingsFlyout.IsOpen = false;
            Properties.Settings.Default.Save();
        }

        private void TabLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
