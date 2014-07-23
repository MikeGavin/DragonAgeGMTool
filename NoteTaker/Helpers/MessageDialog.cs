using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace NoteTaker.Helpers
{
    class MessageDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
    

    static class MetroMessageBox
    {
        public static void Show(string title, string message, bool results = false)
        {
            var dialog = new MessageDialog()
            {              
                Title = title,
                Message = message,
            };
            Messenger.Default.Send(dialog);
        }

        public static async Task<MessageDialogResult> ShowMessage(string message, MessageDialogStyle dialogStyle)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            return await metroWindow.ShowMessageAsync(
                "MY TITLE", message, dialogStyle, metroWindow.MetroDialogOptions);
        }

        public static async Task test()
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
            await metroWindow.ShowMessageAsync("Test","Test!!!", MessageDialogStyle.Affirmative, metroWindow.MetroDialogOptions);
        }

    }
}
