using System;
namespace Scrivener.Model
{
    public interface INote
    {
        Guid Guid { get; }
        string Text { get; set; }
        string Title { get; set; }
        Nullable<DateTime> LastUpdated { get; }
    }
}
