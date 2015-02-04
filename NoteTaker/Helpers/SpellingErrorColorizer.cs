﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
 


using NHunspell;
using System.IO;
using System.Collections.Generic;
namespace Scrivener.Helpers
{
    public class SpellingErrorColorizer : DocumentColorizingTransformer
    {
        private static readonly TextBox staticTextBox = new TextBox();
        private readonly TextDecorationCollection collection;
        private string customDictionary = System.IO.Path.Combine(new Scrivener.Model.DeploymentData(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))).SettingsFolder, "CustomDictionary.lex");

        public SpellingErrorColorizer()
        {
            // Create the Text decoration collection for the visual effect - you can get creative here
            collection = new TextDecorationCollection();
            var dec = new TextDecoration();
            dec.Pen = new Pen {Thickness = 1, DashStyle = DashStyles.DashDot, Brush = new SolidColorBrush(Colors.Red)};
            dec.PenThicknessUnit = TextDecorationUnit.FontRecommended;
            collection.Add(dec);

            // Set the static text box properties
            staticTextBox.AcceptsReturn = true;
            staticTextBox.AcceptsTab = true;
            staticTextBox.SpellCheck.IsEnabled = true;

            
        }

        protected override void ColorizeLine(DocumentLine line)
        {


            var aff = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Resources/en_US.aff", UriKind.Absolute);
            var dic = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Resources/en_US.dic", UriKind.Absolute);

            var engine = new NHunspell.Hunspell(aff.LocalPath, dic.LocalPath);

            var text = CurrentContext.Document.GetText(line).ToString();
            string[] test = text.Split(' ');

            var list = new List<string>();
            foreach (var word in test)
            {
                if(engine.Spell(word))
                {
                    list.Add(word);
                }
            }
            foreach (var word in list)
            {
                int lineStartOffset = line.Offset;
                int starts = 0;
                int index;
                while ((index = text.IndexOf("AvalonEdit", starts)) >= 0)
                {

                    base.ChangeLinePart(
                        lineStartOffset + index, // startOffset
                        lineStartOffset + index + 10, // endOffset
                        (VisualLineElement element) =>
                        {
                            element.TextRunProperties.SetTextDecorations(collection);
                        });
                    starts = index + 1; // search for next occurrence
                }
            }



            //lock (staticTextBox)
            //{
            //    var dictionaries = SpellCheck.GetCustomDictionaries(staticTextBox);
            //    dictionaries.Add(new Uri(@"pack://application:,,,/Scrivener;component/Resources/defaultDictionary.lex"));
            //    if (System.IO.File.Exists(customDictionary))
            //    {
            //        dictionaries.Add(new Uri(customDictionary));
            //    }
            //    staticTextBox.Text = CurrentContext.Document.GetText(line);
            //    int start = 0;
            //    int end = line.Length;
            //    start = staticTextBox.GetNextSpellingErrorCharacterIndex(start, LogicalDirection.Forward);
            //    while (start < end)
            //    {
            //        if (start == -1)
            //            break;

            //        int wordEnd = start + staticTextBox.GetSpellingErrorLength(start);

            //        SpellingError error = staticTextBox.GetSpellingError(start);
            //        if (error != null)
            //        {
            //            base.ChangeLinePart(start, wordEnd, (VisualLineElement element) => element.TextRunProperties.SetTextDecorations(collection));
            //        }

            //        start = staticTextBox.GetNextSpellingErrorCharacterIndex(wordEnd, LogicalDirection.Forward);
            //    }
            //}

            //var aff = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Resources/en_US.aff", UriKind.Absolute);
            //var dic = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) +  "/Resources/en_US.dic", UriKind.Absolute);

            //var engine = new Hunspell(aff.LocalPath, dic.LocalPath);

            //System.Diagnostics.Debugger.Break();

            //int lineStartOffset = line.Offset;
            //string text = CurrentContext.Document.GetText(line);
            //int starts = 0;
            //int index;
            //while ((index = text.IndexOf("AvalonEdit", starts)) >= 0)
            //{

            //    base.ChangeLinePart(
            //        lineStartOffset + index, // startOffset
            //        lineStartOffset + index + 10, // endOffset
            //        (VisualLineElement element) =>
            //        {
            //            element.TextRunProperties.SetTextDecorations(collection);
            //        });
            //    starts = index + 1; // search for next occurrence
            //}
        }
    }
}