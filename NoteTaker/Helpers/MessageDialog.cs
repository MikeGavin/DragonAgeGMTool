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
            var DB = Model.DatabaseStorage.Instance;
            var metroWindow = (Application.Current.MainWindow as MetroWindow);          
            var mySettings = new MetroDialogSettings()
            {
                
                AffirmativeButtonText = DB.Roles[0].Name,
                NegativeButtonText = DB.Roles[1].Name,
                FirstAuxiliaryButtonText = DB.Roles[2].Name,
                SecondAuxiliaryButtonText = "Cancel",
                ColorScheme = MetroDialogColorScheme.Accented
            };
            
            MessageDialogResult temp = await metroWindow.ShowMessageAsync("Choose Role.", "No current role was found. Please choose from the following:", MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary,mySettings);
            if (temp == MessageDialogResult.Affirmative)
                return DB.Roles[0];
            else if (temp == MessageDialogResult.Negative)
                return DB.Roles[1];
            else if (temp == MessageDialogResult.FirstAuxiliary)
                return DB.Roles[2];
            else
            {
                return null;
            }
        }

        public static async Task ShowProgress(string title, string message)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;

            var controler = await metroWindow.ShowProgressAsync("UPDATING", "Downloading Database");
            
        }


    }
}
