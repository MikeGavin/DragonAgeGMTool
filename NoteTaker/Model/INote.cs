using System;
namespace Scrivener.Model
{
    interface INote
    {
        Guid Guid { get; }
        string Text { get; set; }
        string Title { get; set; }
    }
}
