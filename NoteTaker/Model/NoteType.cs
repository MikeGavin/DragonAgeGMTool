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
        public Nullable<DateTime> LastUpdated{ get; set; }

        public NoteType(Guid guid, string title, string text)
        {
            Guid = guid;                       
            Title = title;
            Text = text;
        }
        public NoteType()
        {
            Guid = Guid.NewGuid();
        }
        public NoteType(string title, string text)
        {
            Guid = Guid.NewGuid();
            Title = title;
            Text = text;
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
