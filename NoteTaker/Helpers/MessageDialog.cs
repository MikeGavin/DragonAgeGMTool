using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace Scrivener.Helpers
{
    class MessageDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
    

    static class MetroMessageBox
    {
        //public static void Show(string title, string message, bool results = false)
        //{
        //    var dialog = new MessageDialog()
        //    {              
        //        Title = title,
        //        Message = message,
        //    };
        //    Messenger.Default.Send(dialog);
        //}

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

        public static async Task<Model.RoleItem> GetRole()
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);          
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = Model.Settings.Roles[0].Name,
                NegativeButtonText = Model.Settings.Roles[1].Name,
                FirstAuxiliaryButtonText = "Cancel",
                ColorScheme = MetroDialogColorScheme.Accented
            };
            
            MessageDialogResult temp = await metroWindow.ShowMessageAsync("Choose Role.", "No current role was found. Please choose from the following:", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,mySettings);
            if (temp == MessageDialogResult.Affirmative)
                return Model.Settings.Roles[0];
            else if (temp == MessageDialogResult.Negative)
                return Model.Settings.Roles[1];
            else
            {
                return null;
            }
        }


    }
}
