using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public static class Settings
    {

        static NLog.Logger nlog = NLog.LogManager.GetCurrentClassLogger();

        static void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) { PropertyChanged(new object(), new PropertyChangedEventArgs(prop)); }
        }
        public static event PropertyChangedEventHandler PropertyChanged;
        
        public static string Role { get { return Properties.Settings.Default.Role_Current; } set { Properties.Settings.Default.Role_Current = value; RaisePropertyChanged(); } }

        public static async void Load()
        {
            if (Role == null || Role == string.Empty)
            {
                //await Scrivener.Helpers.MetroMessageBox.Show("Test", "Test");
            }
        }
    }
}
