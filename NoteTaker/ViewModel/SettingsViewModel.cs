using Scrivener.Helpers;
using Scrivener.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.ViewModel
{
    public class SettingsViewModel : GalaSoft.MvvmLight.ViewModelBase
    {

        static NLog.Logger nlog = NLog.LogManager.GetCurrentClassLogger();

        static void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) { PropertyChanged(new object(), new PropertyChangedEventArgs(prop)); }
        }
        public static event PropertyChangedEventHandler PropertyChanged;

        private static ObservableCollection<RoleItem> _roles;
        public static ObservableCollection<RoleItem> Roles { get { return _roles ?? (_roles = LocalDatabase.ReturnRoles()); } }

        public static RoleItem CurrentRole { get { return Properties.Settings.Default.Role_Current; } set { Properties.Settings.Default.Role_Current = value; RaisePropertyChanged(); } }

        public static bool QuicknotesVisible { get { return Properties.Settings.Default.QuickNotes_Visible; } set { Properties.Settings.Default.QuickNotes_Visible = value; RaisePropertyChanged(); } }

        public static async void Load()
        {
            if (CurrentRole == null)
            {
                CurrentRole = await MetroMessageBox.GetRole();
                Properties.Settings.Default.Save();
                if (CurrentRole == null)
                {
                    await MetroMessageBox.Show(string.Empty, "Apathy is death.");
                    Environment.Exit(0);
                }

            }
        }
    }
}
