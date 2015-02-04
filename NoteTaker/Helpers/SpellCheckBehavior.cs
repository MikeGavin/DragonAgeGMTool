﻿using System;
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
        //private string customDictionary = @"c:\temp\custom.lex";
        private string customDictionary = Path.Combine(new Scrivener.Model.DeploymentData(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))).SettingsFolder, "CustomDictionary.lex");

        protected override void OnAttached()
        {                       
            textEditor = AssociatedObject;
            if (textEditor != null)
            {
                textBox = new TextBox();
                textEditor.ContextMenuOpening += textEditor_ContextMenuOpening;
                textEditor.ContextMenuClosing += textEditor_ContextMenuClosing;
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
                var dictionaries = SpellCheck.GetCustomDictionaries(textBox);
                dictionaries.Add(new Uri(@"pack://application:,,,/Scrivener;component/Resources/defaultDictionary.lex"));
                if (File.Exists(customDictionary))
                {
                    dictionaries.Add(new Uri(customDictionary));
                }

                //Check for spelling errors
                SpellingError error = textBox.GetSpellingError(newCaret);
                int wordStartOffset = textBox.GetSpellingErrorStart(newCaret);
                if (wordStartOffset >= 0)
                {
                    int wordLength = textBox.GetSpellingErrorLength(wordStartOffset);
                    //If there is a spelling mistake
                    if (error != null && error.Suggestions.Count() >= 1)
                    {
                        textEditor.ContextMenu = new ContextMenu();

                        foreach (string err in error.Suggestions)
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
            AddToMenuItem.IsEnabled = true;
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
            using (StreamWriter streamWriter = new StreamWriter(customDictionary, true))
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