using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class NoteType: INote
    {
        private Guid _guid; // used for ID for note saving
        public Guid Guid { get { return _guid; } protected set { _guid = value; } }
        public string Text { get; set; }
        public string Title { get; set; }
        public DateTime LastUpdated{ get; protected set; }
        public int Communication { get; set; }
        public int Speed { get; set; }
        public int Constitution { get; set; }
        public int Cunning { get; set; }
        public int Dexterity { get; set; }
        public int Defense { get; set; }
        public int Magic { get; set; }
        public int Perception { get; set; }
        public int Armor { get; set; }
        public int Strength { get; set; }
        public int Willpower { get; set; }


        public NoteType(int communication, int speed, int constitution, int cunning, int dexterity, int defense, int magic, int perception, int armor, int strength, int willpower)
        {
             Communication = communication;
             Speed = speed;
             Constitution = constitution;
             Cunning = cunning;
             Dexterity = dexterity;
             Defense = defense;
             Magic = magic;
             Perception = perception;
             Armor = armor;
             Strength = strength;
             Willpower = willpower;
        }

        public NoteType(Guid guid, string title, string text, DateTime datetime)
        {
            Guid = guid;                       
            Title = title;
            Text = text;
            LastUpdated = datetime;
        }
        public NoteType()
        {
            Guid = Guid.NewGuid();
            LastUpdated = DateTime.Now;
        }
        public NoteType(string title, string text)
        {
            Guid = Guid.NewGuid();
            Title = title;
            Text = text;
            LastUpdated = DateTime.Now;
        }
        public NoteType(string title, string text, DateTime lastupdated)
        {
            Guid = Guid.NewGuid();
            Title = title;
            Text = text;
            LastUpdated = lastupdated;
        }
    }
}
