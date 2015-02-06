using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
 


using NHunspell;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scrivener.Model;
using System.Text.RegularExpressions;
namespace Scrivener.Helpers
{
    public class SpellingErrorColorizer : DocumentColorizingTransformer
    {
        private readonly TextDecorationCollection collection;
        private string customDictionary = System.IO.Path.Combine(new Scrivener.Model.DeploymentData(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))).SettingsFolder, "CustomDictionary.lex");

        public DatabaseStorage DataB { get; set; }

        public SpellingErrorColorizer()
        {
            // Create the Text decoration collection for the visual effect - you can get creative here
            collection = new TextDecorationCollection();
            var dec = new TextDecoration();
            dec.Pen = new Pen {Thickness = 1, DashStyle = DashStyles.DashDot, Brush = new SolidColorBrush(Colors.Red)};
            dec.PenThicknessUnit = TextDecorationUnit.FontRecommended;
            collection.Add(dec);

            DataB = DatabaseStorage.Instance;           
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            var text = CurrentContext.Document.GetText(line).ToString();

            foreach (var word in DataB.List)
            {
                int lineStartOffset = line.Offset;
                MatchCollection words = Regex.Matches(text, string.Format(@"\b{0}\b", word));
                foreach (Match match in words)
                {
                    base.ChangeLinePart(
                        lineStartOffset + match.Index, // startOffset
                        lineStartOffset + match.Index + match.Length, // endOffset
                        (VisualLineElement element) =>
                        {
                            element.TextRunProperties.SetTextDecorations(collection);
                        });
                }
            }
                              
        }
    }
}
