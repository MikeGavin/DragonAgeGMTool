using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;
using System.Windows.Input;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Scrivener.Helpers
{
    //Orginal from http://www.codeproject.com/Tips/560292/AvalonEdit-and-Spell-check
    //Modifited to reset menu to default and add detaching
    internal class SpellCheckBehavior : Behavior<TextEditor>
    {
        private TextBox textBox;
        private TextEditor textEditor;

        protected override void OnAttached()
        {                       
            textEditor = AssociatedObject;
            if (textEditor != null)
            {
                textBox = new TextBox();
                textEditor.ContextMenuOpening += textEditor_ContextMenuOpening;
                textEditor.ContextMenuClosing += textEditor_ContextMenuClosing;
            }
            base.OnAttached();
        }
        protected override void OnDetaching()
        {
            textEditor = AssociatedObject;
            if (textEditor != null)
            {
                textEditor.ContextMenuOpening -= textEditor_ContextMenuOpening;
                textEditor.ContextMenuClosing -= textEditor_ContextMenuClosing;
            }
            base.OnDetaching();
        }
    
        void textEditor_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            //Reset the context menu
            //textEditor.ContextMenu = defaultMenu;
        }



        private void textEditor_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            TextViewPosition? pos = textEditor.TextArea.TextView.GetPosition(new Point(e.CursorLeft, e.CursorTop));
            //Reset the context menu
            textEditor.ContextMenu = new ContextMenu();   
            if (pos != null)
            {            
   

                //Get the new caret position
                int newCaret = textEditor.Document.GetOffset(pos.Value.Line, pos.Value.Column);
                
                //Text box properties
                textBox.AcceptsReturn = true;
                textBox.AcceptsTab = true;
                textBox.SpellCheck.IsEnabled = true;
                textBox.Text = textEditor.Text;

                //Check for spelling errors
                SpellingError error = textBox.GetSpellingError(newCaret);

                //If there is a spelling mistake
                if (error != null && error.Suggestions.Count() >= 1)
                {                 
                    textEditor.ContextMenu = new ContextMenu();
                    int wordStartOffset = textBox.GetSpellingErrorStart(newCaret);
                    int wordLength = textBox.GetSpellingErrorLength(wordStartOffset);
                    foreach (string err in error.Suggestions)
                    {
                        var item = new MenuItem {Header = err};
                        var t = new Tuple<int, int>(wordStartOffset, wordLength);
                        item.Tag = t;
                        item.Click += item_Click;
                        textEditor.ContextMenu.Items.Add(item);
                    }

                    this.textEditor.ContextMenu.Items.Add(new Separator());
                    //Adding the IgnoreAll menu item
                    MenuItem IgnoreAllMenuItem = new MenuItem();
                    IgnoreAllMenuItem.Header = "Ignore All";
                    IgnoreAllMenuItem.IsEnabled = true;
                    //IgnoreAllMenuItem.Command = EditingCommands.IgnoreSpellingError;
                    //IgnoreAllMenuItem.CommandTarget = textEditor;
                    IgnoreAllMenuItem.Click += (object o, RoutedEventArgs rea) => 
                    {
                        this.AddToDictionary(this.textEditor.SelectedText);
                    };

                    this.textEditor.ContextMenu.Items.Add(IgnoreAllMenuItem);

                }
                else
                {
                    //No Suggestions found, add a disabled NoSuggestions menuitem.
                    this.textEditor.ContextMenu.Items.Add(new MenuItem { Header = "No Suggestions", IsEnabled = false });
                }

                this.textEditor.ContextMenu.Items.Add(new Separator());

                
            }
             
            this.AddStandards();

        }

        //Method to Add text to Dictionary
        private void AddToDictionary(string entry)
        {
            using (StreamWriter streamWriter = new StreamWriter(@"C:\MyCustomDictionary.lex", true))
            {
                streamWriter.WriteLine(entry);
            }
        }

        //Click event of the context menu
        private void item_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item != null)
            {
                var seg = item.Tag as Tuple<int, int>;
                textEditor.Document.Replace(seg.Item1, seg.Item2, item.Header.ToString());
            }
        }

        private void AddStandards()
        {
            //Common Edit MenuItems.

            this.textEditor.ContextMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Undo });
            this.textEditor.ContextMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Redo });

            this.textEditor.ContextMenu.Items.Add(new Separator());

            //Cut
            MenuItem cutMenuItem = new MenuItem();
            cutMenuItem.Command = ApplicationCommands.Cut;
            this.textEditor.ContextMenu.Items.Add(cutMenuItem);
            //Copy
            MenuItem copyMenuItem = new MenuItem();
            copyMenuItem.Command = ApplicationCommands.Copy;
            this.textEditor.ContextMenu.Items.Add(copyMenuItem);
            
            //Paste
            MenuItem pasteMenuItem = new MenuItem();
            pasteMenuItem.Command = ApplicationCommands.Paste;
            this.textEditor.ContextMenu.Items.Add(pasteMenuItem);

            this.textEditor.ContextMenu.Items.Add(new Separator());
            
            //Delete
            MenuItem deleteMenuItem = new MenuItem();
            deleteMenuItem.Command = ApplicationCommands.Delete;
            this.textEditor.ContextMenu.Items.Add(deleteMenuItem);
            
            this.textEditor.ContextMenu.Items.Add(new Separator());
            
            //Select All
            MenuItem selectAllMenuItem = new MenuItem();
            selectAllMenuItem.Command = ApplicationCommands.SelectAll;
            this.textEditor.ContextMenu.Items.Add(selectAllMenuItem);            
        }
    }
}