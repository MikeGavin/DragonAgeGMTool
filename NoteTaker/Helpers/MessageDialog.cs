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
        public bool Results { get; set; }
    }
    

    static class MetroMessageBox
    {
        public static void Show(string title, string message, bool results = false)
        {
            var dialog = new MessageDialog()
            {              
                Title = title,
                Message = message,
                Results = results,
            };
            Messenger.Default.Send(dialog);
        }

    }
}
