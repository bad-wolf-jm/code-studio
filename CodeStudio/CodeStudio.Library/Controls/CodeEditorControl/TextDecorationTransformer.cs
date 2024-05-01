using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace Metrino.Development.Studio.Library.Controls;

class TextDecorationTransformer : DocumentColorizingTransformer
{
    protected override void ColorizeLine(DocumentLine line)
    {
        if (line.LineNumber == 2)
        {
            string lineText = this.CurrentContext.Document.GetText(line);

            int indexOfUnderline = lineText.IndexOf("underline");
            int indexOfStrikeThrough = lineText.IndexOf("strikethrough");

            if (indexOfUnderline != -1)
            {
                ChangeLinePart(
                    line.Offset + indexOfUnderline,
                    line.Offset + indexOfUnderline + "underline".Length,
                    visualLine =>
                    {
                        if (visualLine.TextRunProperties.TextDecorations != null)
                        {
                            var textDecorations = new TextDecorationCollection(visualLine.TextRunProperties.TextDecorations) { TextDecorations.Underline[0] };

                            visualLine.TextRunProperties.SetTextDecorations(textDecorations);
                        }
                        else
                        {
                            visualLine.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
                        }
                    }
                );
            }

            if (indexOfStrikeThrough != -1)
            {
                ChangeLinePart(
                    line.Offset + indexOfStrikeThrough,
                    line.Offset + indexOfStrikeThrough + "strikethrough".Length,
                    visualLine =>
                    {
                        if (visualLine.TextRunProperties.TextDecorations != null)
                        {
                            var textDecorations = new TextDecorationCollection(visualLine.TextRunProperties.TextDecorations) { TextDecorations.Strikethrough[0] };

                            visualLine.TextRunProperties.SetTextDecorations(textDecorations);
                        }
                        else
                        {
                            visualLine.TextRunProperties.SetTextDecorations(TextDecorations.Strikethrough);
                        }
                    }
                );
            }
        }
    }
}
