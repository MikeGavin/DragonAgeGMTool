using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;

namespace Scrivener.Helpers
{
    public class SpellingErrorColorizer : DocumentColorizingTransformer
    {
        private static readonly TextBox staticTextBox = new TextBox();
        private readonly TextDecorationCollection collection;

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
            lock (staticTextBox)
            {
                var dictionaries = SpellCheck.GetCustomDictionaries(staticTextBox);
                dictionaries.Add(new Uri(@"C:\Users\Cain\Desktop\MyCustomDictionary.lex"));
                staticTextBox.Text = CurrentContext.Document.Text;
                int start = line.Offset;
                int end = line.EndOffset;
                start = staticTextBox.GetNextSpellingErrorCharacterIndex(start, LogicalDirection.Forward);
                while (start < end)
                {
                    if (start == -1)
                        break;

                    int wordEnd = start + staticTextBox.GetSpellingErrorLength(start);

                    SpellingError error = staticTextBox.GetSpellingError(start);
                    if (error != null)
                    {
                        base.ChangeLinePart(start, wordEnd,(VisualLineElement element) => element.TextRunProperties.SetTextDecorations(collection));
                    }

                    start = staticTextBox.GetNextSpellingErrorCharacterIndex(wordEnd, LogicalDirection.Forward);
                }
            }
        }
    }
}