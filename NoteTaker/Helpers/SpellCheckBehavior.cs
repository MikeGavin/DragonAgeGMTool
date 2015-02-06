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
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;

namespace Scrivener.Helpers
{
    //Orginal from http://www.codeproject.com/Tips/560292/AvalonEdit-and-Spell-check
    //Modifited to reset menu to default and add detaching
    internal class SpellCheckBehavior : Behavior<TextEditor>
    {
        
        private TextEditor textEditor;
        //private string customDictionary = @"c:\temp\custom.lex";
        //private string customDictionary = Path.Combine(new Scrivener.Model.DeploymentData(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))).SettingsFolder, "CustomDictionary.lex");

        public Scrivener.Model.DatabaseStorage DataB { get; set; }
        private static List<string> _errors = new List<string>();
        public static List<string> Errors { get { return _errors; } }

        Uri aff = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Resources/en_US.aff", UriKind.Absolute);
        Uri dic = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Resources/en_US.dic", UriKind.Absolute);

        NHunspell.Hunspell engine;



        protected override void OnAttached()
        {                       
            textEditor = AssociatedObject;
            if (textEditor != null)
            {
                
                textEditor.ContextMenuOpening += textEditor_ContextMenuOpening;
                textEditor.ContextMenuClosing += textEditor_ContextMenuClosing;

                DataB = Scrivener.Model.DatabaseStorage.Instance;
                engine = new NHunspell.Hunspell(aff.LocalPath, dic.LocalPath);
                textEditor.TextChanged += textEditor_TextChanged;
                

                AssociatedObject.TextArea.TextView.LineTransformers.Add(new SpellingErrorColorizer());
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
                textEditor.TextChanged -= textEditor_TextChanged;
            }
            base.OnDetaching();
        }

        void textEditor_TextChanged(object sender, EventArgs e)
        {
            LiveSpellCheck();
        }

        private void LiveSpellCheck()
        {
            if (textEditor.Text != null && textEditor.Text != string.Empty)
            {
                char test = textEditor.Text[textEditor.Text.Length - 1];
                if (char.IsWhiteSpace(test) || char.IsPunctuation(test))
                {
                    Regex words = new Regex(@"\w+-\w+|[\w\S]+[^\W\d_]", RegexOptions.IgnoreCase);
                    DataB.List.Clear();
                    _errors.Clear();
                    MatchCollection mc = words.Matches(new string(textEditor.Text.Where(c => !char.IsPunctuation(c)).ToArray()));
                    var uniqueMatches = mc.OfType<Match>().Select(m => m.Value).Distinct();
                    var matchlist = uniqueMatches.ToList();

                    foreach (var word in matchlist)
                    {
                        if (word == string.Empty) { return; }
                        if (!DataB.List.Contains(word))
                        {
                            if (!engine.Spell(word))
                            {
                                DataB.List.Add(word);
                            }
                        }
                    }
                }
            }
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

                var wordEnd = TextUtilities.GetNextCaretPosition(textEditor.Document, newCaret, LogicalDirection.Forward, CaretPositioningMode.WordBorder);
                if (wordEnd < 0)
                {
                    wordEnd = textEditor.Text.Length;
                }
                var wordStartOffset = TextUtilities.GetNextCaretPosition(textEditor.Document, newCaret, LogicalDirection.Backward, CaretPositioningMode.WordStart);
                var wordLength = wordEnd - wordStartOffset;
                var word = textEditor.Text.Substring(wordStartOffset, wordLength);
                var suggestions = new List<string>();
                if (!engine.Spell(word))
                {
                    suggestions = engine.Suggest(word);
                }

                //If there is a spelling mistake
                if (suggestions != null && suggestions.Count() >= 1)
                {
                    textEditor.ContextMenu = new ContextMenu();

                    foreach (string err in suggestions)
                    {
                        var item = new MenuItem { Header = err };
                        var t = new Tuple<int, int>(wordStartOffset, wordLength);
                        item.Tag = t;
                        item.Click += item_Click;
                        textEditor.ContextMenu.Items.Add(item);
                    }

                    this.textEditor.ContextMenu.Items.Add(AddToDictionaryMenuItem(wordStartOffset, wordLength));

                }
                else
                {
                    //No Suggestions found, add a disabled NoSuggestions menuitem.
                    this.textEditor.ContextMenu.Items.Add(new MenuItem { Header = "No Suggestions", IsEnabled = false });
                    this.textEditor.ContextMenu.Items.Add(AddToDictionaryMenuItem(wordStartOffset, wordLength));
                }
                this.textEditor.ContextMenu.Items.Add(new Separator());
            }
            this.AddStandards();

        }

        private MenuItem AddToDictionaryMenuItem(int wordStartOffset, int wordLength)
        {
            this.textEditor.ContextMenu.Items.Add(new Separator());
            //Adding the IgnoreAll menu item
            MenuItem AddToMenuItem = new MenuItem();
            AddToMenuItem.Header = "Add to dictionary";
            AddToMenuItem.IsEnabled = false;
            AddToMenuItem.Tag = new Tuple<int, int>(wordStartOffset, wordLength);
            //IgnoreAllMenuItem.Command = EditingCommands.IgnoreSpellingError;
            //IgnoreAllMenuItem.CommandTarget = textEditor;
            AddToMenuItem.Click += (object o, RoutedEventArgs rea) =>
            {
                var item = o as MenuItem;
                var seg = item.Tag as Tuple<int, int>;
                this.textEditor.SelectionStart = seg.Item1;
                this.textEditor.SelectionLength = seg.Item2;
                this.AddToDictionary(this.textEditor.SelectedText);
                textEditor.Document.Replace(seg.Item1, seg.Item2, this.textEditor.SelectedText); //causes refresh for check in underline system
            };
            return AddToMenuItem;
        }

        //Method to Add text to Dictionary
        private void AddToDictionary(string entry)
        {
            //using (StreamWriter streamWriter = new StreamWriter(customDictionary, true))
            //{
            //    streamWriter.WriteLine(entry);
            //}
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