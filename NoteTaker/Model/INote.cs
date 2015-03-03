using System;
namespace Scrivener.Model
{
    public interface INote
    {
        Guid Guid { get; }
        string Text { get; set; }
        string Title { get; set; }
        DateTime LastUpdated { get; }
        int Life { get; set; }
        int Mana { get; set; }
        int Experience { get; set; }
        int Communication { get; set; }
        int Speed { get; set; }
        int Constitution { get; set; }
        int Cunning { get; set; }
        int Dexterity { get; set; }
        int Defense { get; set; }
        int Magic { get; set; }
        int Perception { get; set; }
        int Armor { get; set; }
        int Strength { get; set; }
        int Willpower { get; set; }

    }
}
