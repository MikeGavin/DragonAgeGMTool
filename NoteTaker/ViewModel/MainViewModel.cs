using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Scrivener.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using Scrivener.Helpers;
using System.Collections.Specialized;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Runtime.CompilerServices;
using System.IO;
using System.Reactive.Linq;
using NLog.Targets;
using NLog;
using System.Text;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Interactivity;
using System.Globalization;


namespace Scrivener.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region Boilerplate
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        
        private readonly IDataService _dataService; // Used by MVVMLight 

        private CharacterManager CharMan;
        #endregion

        #region Constructor
        public MainViewModel(IDataService dataService)
        {
            //Retreave previous version settings.
            //http://stackoverflow.com/questions/622764/persisting-app-config-variables-in-updates-via-click-once-deployment/622813#622813

            //Event Listener to auto save notes if application failes through unhandeled expection
            App.Fucked += (s, e) => SaveAllNotes();
            //Application.Current.MainWindow.Closing += (s, e) => SaveAllNotes();

            //Listen for note collection change
            Notes.CollectionChanged += OnNotesChanged;

            //Auto save settings on any change.
            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;

            CharMan = new CharacterManager();

            Database();

            NewName = "";

            MainGridWidth = 400;
        }

        //WindowLoaded runs functions only availalbe after window has loaded and are unavailable in constructor.
        public async void WindowLoaded()
        {
            var openNotes = await noteManager.GetCurrentNotes();
            if (openNotes.Count > 0)
            {
                //    foreach (INote n in openNotes)
                //    {                 
                //        Notes.Add( new NoteViewModel(n));
                //    }
                //    SelectedNote = Notes.FirstOrDefault();
            }
            if (Notes.Count == 0)
            {
                Properties.Settings.Default.WelcomeScreenVis = true;
            }

        } 
        #endregion

        #region Database
        public void Database()
        {
            CharMan.CreateDatebase();
        } 

        public void PassDBValues(INote note)
        {
            CharMan.WriteID = note.ID;
            CharMan.WriteTitle = note.Title;
            CharMan.WriteLife = note.Life;
            CharMan.WriteMana = note.Mana;
            CharMan.WriteExperience = note.Experience;
            CharMan.WriteCommunication = note.Communication;
            CharMan.WriteConstitution = note.Constitution;
            CharMan.WriteCunning = note.Cunning;
            CharMan.WriteDexterity = note.Dexterity;
            CharMan.WriteMagic = note.Magic;
            CharMan.WritePerception = note.Perception;
            CharMan.WriteStrength = note.Strength;
            CharMan.WriteWillpower = note.Willpower;
            CharMan.WriteSpeed = note.Speed;
            CharMan.WriteDefense = note.Defense;
            CharMan.WriteArmor = note.Armor;
            CharMan.WriteGold = note.Gold;
            CharMan.WriteSilver = note.Silver;
            CharMan.WriteCopper = note.Copper;
        }
        #endregion

        #region Public Properties
        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
        
        private double _mainGridWidth;
        public double MainGridWidth { get { return _mainGridWidth; } set { _mainGridWidth = value; RaisePropertyChanged(); } }

        public double TabWidth { get { return Properties.Settings.Default.CharTabWidth; } set { Properties.Settings.Default.CharTabWidth = value; RaisePropertyChanged(); } }

        private double _mainMinWidth;
        public double MainMinWidth { get { return MainGridWidth + TabWidth; } protected set { _mainMinWidth = value; RaisePropertyChanged(); } }

        private ObservableCollection<CharacterItem> _characterList;
        public ObservableCollection<CharacterItem> CharacterList { get { return _characterList; } set { _characterList = value; RaisePropertyChanged(); } }

        private CharacterItem _selectedCharacter;
        public CharacterItem SelectedCharacter { get { return _selectedCharacter; } set { _selectedCharacter = value; RaisePropertyChanged(); } }

        public bool NewEnabled { get { return Properties.Settings.Default.NewEnabledBool; } set { Properties.Settings.Default.NewEnabledBool = value; RaisePropertyChanged(); } }
        public bool OpenEnabled { get { return Properties.Settings.Default.OpenEnabledBool; } set { Properties.Settings.Default.OpenEnabledBool = value; RaisePropertyChanged(); } }
        public bool SaveEnabled { get { return Properties.Settings.Default.SaveEnabledBool; } set { Properties.Settings.Default.SaveEnabledBool = value; RaisePropertyChanged(); } }

        #endregion

        #region Character System

        //Note Collection
        private MTObservableCollection<NoteViewModel> _Notes = new MTObservableCollection<NoteViewModel>();
        public MTObservableCollection<NoteViewModel> Notes { get { return _Notes; } set { _Notes = value; RaisePropertyChanged(); } }
        private INote _SelectedNote;
        public INote SelectedNote { get { return _SelectedNote; } set { _SelectedNote = value; RaisePropertyChanged(); } }

        //Closing of notes
        void OnNotesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (NoteViewModel note in e.NewItems)
                {
                    note.RequestClose += this.OnNoteRequestClose;
                    note.SaveNoteRequest += note_SaveNoteRequest;
                }

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (NoteViewModel workspace in e.OldItems)
                {
                    workspace.RequestClose -= this.OnNoteRequestClose;
                    workspace.SaveNoteRequest -= note_SaveNoteRequest;
                }

        }

        private void note_SaveNoteRequest(object sender, EventArgs e)
        {
            // this uses a task to save every time a change is detected in order to prevent sluggy database issues. 
            Task t = Task.Factory.StartNew(async () => 
            {
                await noteManager.SaveCurrent(sender as NoteViewModel);
            });
           
        }
        private void OnNoteRequestClose(object sender, EventArgs e)
        {
            NoteViewModel note = sender as NoteViewModel;
            CloseNote(note);
        }        

        private async Task CloseNote(NoteViewModel note)
        {
            if (Properties.Settings.Default.CharacterNameBoxVis == false)
            {
                Notes.Remove(note);

                if (Notes.Count == 0)
                {
                    Properties.Settings.Default.WelcomeScreenVis = true;
                    SaveEnabled = false;
                    Properties.Settings.Default.Save();
                }
            }

            #region CloseWarning
            //if (Properties.Settings.Default.Saveonclose == false)
            //{
            //    if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            //    {
            //        var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
            //        if (result == true)
            //        {
            //            await SetLastSaveClose(note);
            //        }
            //    }
            //    else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            //    {
            //        await SetLastSaveClose(note);
            //    }

            //    if (Notes.Count == 0)
            //    {
            //        NewNote();
            //    }
            //}
            //else if (Properties.Settings.Default.Saveonclose == true && SelectedNote.Text != "")
            //{
            //    if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            //    {
            //        var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
            //        if (result == true)
            //        {
            //            await SetLastSaveClose(note);
            //        }
            //    }
            //    else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            //    {
            //        await SetLastSaveClose(note);
            //    }

            //    if (Notes.Count == 0)
            //    {
            //        NewNote();
            //    }

            //}
            //else
            //{
            //    if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            //    {
            //        var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
            //        if (result == true)
            //        {
            //            await SetLastSaveClose(note);
            //        }
            //    }
            //    else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            //    {
            //        await SetLastSaveClose(note);
            //    }

            //    if (Notes.Count == 0)
            //    {
            //        NewNote();
            //    }
            //} 
            #endregion

        }
        
        public string NewName { get { return Properties.Settings.Default.NewName; } set { if (value != Properties.Settings.Default.NewName) { Properties.Settings.Default.NewName = value; } RaisePropertyChanged(); } }
        
        private RelayCommand<INote> _newNameCommand;
        public RelayCommand<INote> NewNameCommand { get { return _newNameCommand ?? (_newNameCommand = new RelayCommand<INote>((pram)=>NameChange(pram))); } }
        public void NameChange(INote note)
        {
            if (NewName != "")
            {
                SelectedNote.Title = NewName;
                Properties.Settings.Default.CharacterNameBoxVis = false;
                NewName = "";
                NewEnabled = true;
                OpenEnabled = true;
                if (Notes.Count != 0)
                {
                    SaveEnabled = true;
                }

                PassDBValues(note);                
            }
        }

        private RelayCommand _closeNameCommand;
        public RelayCommand CloseNameCommand { get { return _closeNameCommand ?? (_closeNameCommand = new RelayCommand(CloseName)); } }
        public void CloseName()
        {
            Properties.Settings.Default.CharacterNameBoxVis = false;
            NewName = "";
            NewEnabled = true;
            OpenEnabled = true;
            if(Notes.Count != 0)
            {
                SaveEnabled = true;
            }
        }

        private RelayCommand<INote> _saveCharacterCommand;
        public RelayCommand<INote> SaveCharacterCommand { get { return _saveCharacterCommand ?? (_saveCharacterCommand = new RelayCommand<INote>((pram)=>SaveCharacter(pram))); } }
        public void SaveCharacter(INote note)
        {
            try
            {
                    PassDBValues(note);
                    CharMan.SaveCharacter();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private RelayCommand _saveAllCharactersCommand;
        public RelayCommand SaveAllCharactersCommand { get { return _saveAllCharactersCommand ?? (_saveAllCharactersCommand = new RelayCommand(SaveAllCharacters)); } }

        private void SaveAllCharacters()
        {
            foreach (var n in Notes)
            {
                SaveCharacter(n);
            }
        }

        private RelayCommand _closeAllNotesCommand;
        public RelayCommand CloseAllNotesCommand { get { return _closeAllNotesCommand ?? (_closeAllNotesCommand = new RelayCommand(CloseAllNotes)); } }

        private async void CloseAllNotes()
        {
            foreach (NoteViewModel n in Notes.ToList())
            {
                await CloseNote(n);
            }
        }

        //New Notes
        private RelayCommand<INote> _newNoteCommand;
        public RelayCommand<INote> NewNoteCommand { get { return _newNoteCommand ?? (_newNoteCommand = new RelayCommand<INote>((parm) => NewNote("RelayCommand", parm) )); } }
        private async void NewNote([CallerMemberName]string memberName = "", INote note = null )
        {
            CharMan.GetID();
            Properties.Settings.Default.IDCount = CharMan.CurrentIDCount;

            Properties.Settings.Default.WelcomeScreenVis = false;
            Properties.Settings.Default.Save();
            await Task.Factory.StartNew(() =>
            {
                log.Debug("{0} ran NewNote", memberName);
                Notes.Add(new NoteViewModel(note));
                SelectedNote = Notes.Last();
            });

            Properties.Settings.Default.CharacterNameBoxVis = true;
            Properties.Settings.Default.SetCharacterFocus = true;
            Properties.Settings.Default.SetCharacterFocus = false;
            Properties.Settings.Default.Save();
            NewEnabled = false;
            OpenEnabled = false;
            SaveEnabled = false;
        }

        private RelayCommand _openCharacterBoxCommand;
        public RelayCommand OpenCharacterBoxCommand { get { return _openCharacterBoxCommand ?? (_openCharacterBoxCommand = new RelayCommand(OpenCharacterBox)); } }

        private void OpenCharacterBox()
        {
            if (Properties.Settings.Default.OpenCharacterBoxVis == false)
            {
                CharacterList = CharMan.ReturnCharacters();

                Properties.Settings.Default.WelcomeScreenVis = false;
                Properties.Settings.Default.OpenCharacterBoxVis = true;
                NewEnabled = false;
                SaveEnabled = false;
                OpenEnabled = false;
                Properties.Settings.Default.Save();
            }
            else if (Properties.Settings.Default.OpenCharacterBoxVis == true && Notes.Count == 0)
            {
                Properties.Settings.Default.OpenCharacterBoxVis = false;
                Properties.Settings.Default.WelcomeScreenVis = true;
                NewEnabled = true;
                SaveEnabled = true;
                OpenEnabled = true;
            }
            else if (Properties.Settings.Default.OpenCharacterBoxVis == true)
            {
                Properties.Settings.Default.OpenCharacterBoxVis = false;
                NewEnabled = true;
                SaveEnabled = true;
                OpenEnabled = true;
            }
        }

        private RelayCommand<INote> _openCharacterCommand;
        public RelayCommand<INote> OpenCharacterCommand { get { return _openCharacterCommand ?? (_openCharacterCommand = new RelayCommand<INote>((parm) => OpenCharacter("RelayCommand", parm))); } }
        private void OpenCharacter([CallerMemberName]string memberName = "", INote note = null)
        {
            Notes.Add(new NoteViewModel(note));
            SelectedNote = Notes.Last();
            SelectedNote.ID = SelectedCharacter.ID;
            SelectedNote.Title = SelectedCharacter.Title;
            SelectedNote.Life = SelectedCharacter.Life;
            SelectedNote.Mana = SelectedCharacter.Mana;
            SelectedNote.Experience = SelectedCharacter.Experience;
            SelectedNote.Communication = SelectedCharacter.Communication;
            SelectedNote.Speed = SelectedCharacter.Speed;
            SelectedNote.Constitution = SelectedCharacter.Constitution;
            SelectedNote.Cunning = SelectedCharacter.Cunning;
            SelectedNote.Dexterity = SelectedCharacter.Dexterity;
            SelectedNote.Defense = SelectedCharacter.Defense;
            SelectedNote.Magic = SelectedCharacter.Magic;
            SelectedNote.Perception = SelectedCharacter.Perception;
            SelectedNote.Armor = SelectedCharacter.Armor;
            SelectedNote.Strength = SelectedCharacter.Strength;
            SelectedNote.Willpower = SelectedCharacter.Willpower;
            SelectedNote.Gold = SelectedCharacter.Gold;
            SelectedNote.Silver = SelectedCharacter.Silver;
            SelectedNote.Copper = SelectedCharacter.Copper;


            OpenCharacterBox();

            SaveEnabled = true;            
        }

        private RelayCommand _deleteCharacterCommand;
        public RelayCommand DeleteCharacterCommand { get { return _deleteCharacterCommand ?? (_deleteCharacterCommand = new RelayCommand(DeleteCharacter)); } }

        private void DeleteCharacter()
        {
            CharMan.DeleteIndex = SelectedCharacter.ID;
            CharMan.DBDeleteCharacter();
            CharacterList = CharMan.ReturnCharacters();
        }

        

        private INote lastClosedNote;
        //Recall Notes
        private RelayCommand<string> _RecallNoteCommand;
        public RelayCommand<string> RecallNoteCommand { get { return _RecallNoteCommand ?? (_RecallNoteCommand = new RelayCommand<string>(async (parm) => await RecallNote("RecallNote"))); } }
        private async Task RecallNote([CallerMemberName]string memberName = "")
        {
            await Task.Factory.StartNew(() => 
            {
                if (lastClosedNote != null)
                {
                    Notes.Add(lastClosedNote as NoteViewModel);
                    SelectedNote = Notes.Last();
                    lastClosedNote = null;
                }
            });
        }

        #endregion

        #region ToolBar
 
        private RelayCommand _Settingsbuttoncommand;
        public RelayCommand Settingsbuttoncommand { get { return _Settingsbuttoncommand ?? (_Settingsbuttoncommand = new RelayCommand(Settingsbutton)); } }
        public void Settingsbutton()
        {
            if (Properties.Settings.Default.SettingsExpanded == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
            }
            else
            {
                Properties.Settings.Default.SettingsExpanded = false;
            }
        }

        private RelayCommand _SettingsExpandCommand;
        public RelayCommand SettingsExpandCommand { get { return _SettingsExpandCommand ?? (_SettingsExpandCommand = new RelayCommand(SettingsExpand)); } }
        public void SettingsExpand()
        {            
            if (Properties.Settings.Default.SettingsExpanded == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
                Properties.Settings.Default.SettingsSelected = true;
            }
            else if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.SettingsSelected == false)
            {
                Properties.Settings.Default.SettingsSelected = true;
            }
            else
            {
                Properties.Settings.Default.SettingsExpanded = false;
            }
        }
        
        #endregion

        #region Call history

        private NoteManager noteManager = new NoteManager();

        public void Saveallnotesonclose(object sender, CancelEventArgs e)
        {
            SaveAllNotes();
        }

        private async void SaveAllNotes()
        {
            //foreach (NoteViewModel n in Notes)
            //{
            //    if (n.Text != "" && n.Text != Properties.Settings.Default.Default_Note_Template)
            //    {
            //        await noteManager.SaveCurrent(n);
            //    }
            //}

        }

        private async Task SetLastSaveClose(NoteViewModel note)
        {
            //await Task.Factory.StartNew(async () =>
            //{
            //    if (note.Text != "" && note.Text != Properties.Settings.Default.Default_Note_Template)
            //    {
            //        await noteManager.SaveCurrent(note);
            //        lastClosedNote = note;
            //        await noteManager.ArchiveCurrent(note);
            //    }
            //    Notes.Remove(note);                
            //});
        }

        private ObservableCollection<NoteType> _HistoricNotes;
        public ObservableCollection<NoteType> HistoricNotes { get { return _HistoricNotes; } set { _HistoricNotes = value; RaisePropertyChanged();  } }
        private NoteType _SelectedHistoryNote;
        public NoteType SelectedHistoryNote { get { return _SelectedHistoryNote; } set { _SelectedHistoryNote = value; RaisePropertyChanged(); } }
        public Nullable<DateTime> HistoryDate { get; set; }
        private RelayCommand _HistoryLookupCommand;
        public RelayCommand HistoryLookupCommand { get { return _HistoryLookupCommand ?? (_HistoryLookupCommand = new RelayCommand(GetHistory)); } }
        private async void GetHistory()
        {
            HistoricNotes = await noteManager.GetArchivedNotes(HistoryDate ?? DateTime.Now);
        }
        
        #endregion
        
    }
}