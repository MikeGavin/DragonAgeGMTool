using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using Scrivener.ViewModel;

namespace Scrivener.Helpers
{
    class MessageDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
    

    static class MetroMessageBox
    {
        public static async Task<bool> ShowResult(string title, string message)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            MessageDialogResult temp = await metroWindow.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, metroWindow.MetroDialogOptions);
            if (temp == MessageDialogResult.Affirmative)
                return true;
            else
                return false;
        }

        public static async Task Show(string title, string message)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
            await metroWindow.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, metroWindow.MetroDialogOptions);
        }

        public static async Task GetRole()
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);          
            var mySettings = new MetroDialogSettings()
            {
                
                AffirmativeButtonText = "",
                NegativeButtonText = "",
                FirstAuxiliaryButtonText = "",
                SecondAuxiliaryButtonText = "",
                ColorScheme = MetroDialogColorScheme.Accented
            };
            
            MessageDialogResult temp = await metroWindow.ShowMessageAsync("Choose Role.", "No current role was found. Please choose from the following:", MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary,mySettings);
            if (temp == MessageDialogResult.Affirmative)
             {
              
            }
            else if (temp == MessageDialogResult.Negative)
            {
              
            }
            else if (temp == MessageDialogResult.FirstAuxiliary)
            {
              
            }
            else
            {
              
            }
        }

    }
}
