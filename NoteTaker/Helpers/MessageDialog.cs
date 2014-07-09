using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTaker.Helpers
{
    class MessageDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    static class MetroMessageBox
    {
        public static void Show(string title, string message)
        {
            var dialog = new MessageDialog()
            {
                Title = title,
                Message = message,
            };
            Messenger.Default.Send(dialog);
        }
    }
}
