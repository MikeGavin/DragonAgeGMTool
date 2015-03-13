using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using GalaSoft.MvvmLight;
using Scrivener.Model;
using Scrivener.Helpers;
using System.IO;
using System.Windows.Input;

namespace Scrivener.ViewModel
{
    public class NoteViewModel : ViewModelBase, INote
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
      
        public NoteViewModel(INote incomingNote = null)
        {
            if (incomingNote == null)
            {
                ID = Properties.Settings.Default.IDCount;
                Life = 0;
                Mana = 0;
                Experience = 0;
                Communication = 0;
                Constitution = 0;
                Cunning = 0;
                Dexterity = 0;
                Magic = 0;
                Perception = 0;
                Strength = 0;
                Willpower = 0;
                Speed = 0;
                Defense = 0;
                Armor = 0;
                Gold = 0;
                Silver = 0;
                Copper = 0;                
                
                Title = string.Format("New Character");
                
                //LastUpdated = DateTime.Now;
            }
            else
            {
                //Guid = incomingNote.Guid;
                //Text = incomingNote.Text;
                //Title = incomingNote.Title;
                ////_titlechanged = true;
                //LastUpdated = incomingNote.LastUpdated;
            }

        }

        #region Public Properties

        private int id;
        public int ID { get { return id; } set { id = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private string title;
        public string Title { get { return title; } set { title = value; RaisePropertyChanged(); } }
        private int life;
        public int Life { get { return life; } set { life = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int mana;
        public int Mana { get { return mana; } set { mana = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int experience;
        public int Experience { get { return experience; } set { experience = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int communication;
        public int Communication { get { return communication; } set { communication = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int speed;
        public int Speed { get { return speed; } set { speed = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int constitution;
        public int Constitution { get { return constitution; } set { constitution = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int cunning;
        public int Cunning { get { return cunning; } set { cunning = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int dexterity;
        public int Dexterity { get { return dexterity; } set { dexterity = value; RaisePropertyChanged(); } }
        private int defense;
        public int Defense { get { return defense; } set { defense = value; RaisePropertyChanged(); } }
        private int magic;
        public int Magic { get { return magic; } set { magic = value; RaisePropertyChanged(); } }
        private int perception;
        public int Perception { get { return perception; } set { perception = value; RaisePropertyChanged(); } }
        private int armor;
        public int Armor { get { return armor; } set { armor = value; RaisePropertyChanged(); } }
        private int strength;
        public int Strength { get { return strength; } set { strength = value; RaisePropertyChanged(); } }
        private int willpower;
        public int Willpower { get { return willpower; } set { willpower = value; RaisePropertyChanged(); } }
        private int gold;
        public int Gold { get { return gold; } set { gold = value; RaisePropertyChanged(); } }
        private int silver;
        public int Silver { get { return silver; } set { silver = value; RaisePropertyChanged(); } }
        private int copper;
        public int Copper { get { return copper; } set { copper = value; RaisePropertyChanged(); } }
        private DateTime _lastUpdated;
        public DateTime LastUpdated { get { return _lastUpdated; } protected set { _lastUpdated = value; RaisePropertyChanged(); } }

        #endregion        
        
        #region EventBased Actions

        //Text change events for note
        public void RaiseNoteSave()
        {
            if (SaveNoteRequest != null) { SaveNoteRequest(this, new EventArgs()); }
        }
        public event EventHandler SaveNoteRequest;

        internal void RaiseTextChanged()
        {
            if (TextChanged != null) { TextChanged(this, new EventArgs()); }
        }
        public event EventHandler TextChanged;
        
        public event EventHandler RequestClose;
        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        private RelayCommand _closeNoteCommand;
        public RelayCommand CloseNoteCommand { get { return _closeNoteCommand ?? (_closeNoteCommand = new RelayCommand(OnRequestClose)); } }

        #endregion

    }
}
