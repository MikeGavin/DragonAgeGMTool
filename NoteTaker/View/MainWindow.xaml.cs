﻿using System.Windows;
using Scrivener.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Windows.Input;
using System.Windows.Controls;
using System;

using System.Linq;
using Scrivener.UserControls;
using GalaSoft.MvvmLight.Command;


namespace Scrivener.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow

    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.Loaded += MainWindow_Loaded;

            if (Properties.Settings.Default.Role == -1)
            {
                LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
            }
  
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            //if (Properties.Settings.Default.Role == -1)
            //{
            //    this.Hide();
            //    var profile_window = new Role_UI();
            //    profile_window.Show();
            //}

                Closing += (s, e) => ViewModelLocator.Cleanup();               


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

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var test = new ViewModelLocator();
            test.Main.WindowLoaded();
            
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

        private void Roleupdated(object sender, SelectionChangedEventArgs e)
        {
            if (Properties.Settings.Default.Role == 0)
            {
                //LayoutRoot.Visibility = System.Windows.Visibility.Visible;
            }
        }        
    }
}
