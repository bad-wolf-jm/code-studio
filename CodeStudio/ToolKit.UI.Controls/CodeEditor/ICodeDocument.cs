using System.ComponentModel;

namespace Metrino.Development.Studio.Library.Controls;

public struct TextPosition
{
    public int Line;
    public int Column;

    public TextPosition(int line, int column)
    {
        Line = line;
        Column = column;
    }
}

public interface ICodeDocument : INotifyPropertyChanged
{
    string Text { get; set; }
    int LineCount { get; }
    void UpdateLines();
    public string GetLine(int i);
    public int GetLineLength(int i);
    public TextPosition GetPosition(int offset);
    public int GetOffset(TextPosition i);
    public void Insert(int offset, string @string);
    public void Insert(TextPosition position, string @string);
    public void Replace(int offset, int length, string @string);
    public void Replace(TextPosition position, int length, string @string);
    TextHighlight[] GetHighlights(int line, int columnOffset, int length);
}
