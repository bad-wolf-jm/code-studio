using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace ToolKit.UI.Controls;

public interface ICodeDocumentView : INotifyPropertyChanged
{
    int Line { get; }
    int Column { get; }

    int LineOffset { get; }
    int ColumnOffset { get; }
    int NumDisplayedLines { get; }

    ICodeDocument Document { get; }

    void MoveToTop();
    void MoveToBottom();
    void MoveUp();
    void MoveUpParagraph();
    void MoveDown();
    void MoveDownParagraph();
    void MoveLeft();
    void MoveLeftWord();
    void MoveRight();
    void MoveRightWord();
    void MoveToBeginningOfLine();
    void MoveToEndOfLine();
    void Resize(int columns, int rows);
    void DeleteCharacter();
    void DeletePreviousCharacter();
    void InsertCharacter(string keySymbol);
}


public partial class CodeDocumentView : ObservableObject, ICodeDocumentView
{
    public ICodeDocument Document { get; init; }

    public int Line { get; private set; }
    public int Column { get; private set; }
    public int RequestedColumn { get; private set; }

    public int LineOffset { get; private set; }
    public int ColumnOffset { get; private set; }
    public int NumDisplayedLines { get; private set; }
    public int NumDisplayedCharacters { get; private set; }

    public CodeDocumentView(ICodeDocument newValue)
    {
        Document = newValue;
    }

    void MoveHorizontal(int characters)
    {
        var textLineLength = Document.GetLineLength(Line + LineOffset);
        RequestedColumn = Column + characters;

        if (RequestedColumn >= textLineLength)
            RequestedColumn = textLineLength - 1;

        if (RequestedColumn < 0)
        {
            int offset = RequestedColumn;
            ColumnOffset = Math.Max(ColumnOffset + offset, 0);
            RequestedColumn -= offset;
            Column = RequestedColumn;
        }

        if (RequestedColumn >= NumDisplayedCharacters)
        {
            int offset = RequestedColumn - NumDisplayedCharacters + 1;
            ColumnOffset += offset;
            Column -= offset;
            RequestedColumn -= offset;
        }

        Column = RequestedColumn;
    }

    public void MoveLeft()
        => MoveHorizontal(-1);


    public void MoveRight()
        => MoveHorizontal(1);


    public void MoveLeftWord()
    {
        throw new System.NotImplementedException();
    }

    public void MoveRightWord()
    {
        throw new System.NotImplementedException();
    }

    public void MoveToBeginningOfLine()
        => MoveHorizontal(-Document.GetLineLength(Line + LineOffset));


    public void MoveToEndOfLine()
        => MoveHorizontal(Document.GetLineLength(Line + LineOffset));


    void MoveVertical(int lines)
    {
        var newPosition = Line + lines;
        if (newPosition >= NumDisplayedLines)
        {
            var offset = newPosition - NumDisplayedLines + 1;
            LineOffset = Math.Min(LineOffset + offset, Document.LineCount - 8);

            if (newPosition + LineOffset >= Document.LineCount)
                newPosition = Document.LineCount - 1 - LineOffset;

            Line = Math.Min(newPosition, NumDisplayedLines - 1);

            var textLineLength = Document.GetLineLength(Line + LineOffset);
            var lastColumn = Math.Min(textLineLength, NumDisplayedCharacters - 1);
            Column = Math.Min(RequestedColumn, lastColumn);
        }
        else if (newPosition < 0)
        {
            var offset = newPosition;
            LineOffset = Math.Max(LineOffset + offset, 0);

            if (newPosition + LineOffset >= Document.LineCount)
                newPosition = Document.LineCount - 1 - LineOffset;

            Line = Math.Max(newPosition, 0);

            var textLineLength = Document.GetLineLength(Line + LineOffset);
            var lastColumn = Math.Min(textLineLength, NumDisplayedCharacters - 1);
            Column = Math.Min(RequestedColumn, lastColumn);
        }
        else
        {
            if (newPosition + LineOffset >= Document.LineCount)
                newPosition = Document.LineCount - 1 - LineOffset;

            Line = Math.Min(newPosition, NumDisplayedLines - 1);

            var textLineLength = Document.GetLineLength(Line + LineOffset);
            var lastColumn = Math.Min(textLineLength, NumDisplayedCharacters - 1);
            Column = Math.Min(RequestedColumn, lastColumn);
        }
    }

    public void MoveDown()
        => MoveVertical(1);

    public void MoveUp()
        => MoveVertical(-1);

    public void MoveDownParagraph()
    {
        throw new System.NotImplementedException();
    }

    public void MoveUpParagraph()
    {
        throw new System.NotImplementedException();
    }

    public void MoveToTop()
        => MoveVertical(-Document.LineCount);

    public void MoveToBottom()
        => MoveVertical(Document.LineCount);

    public void Resize(int columns, int rows)
    {
        NumDisplayedCharacters = columns;
        NumDisplayedLines = rows;
    }

    public void DeleteCharacter()
    {
        int line = LineOffset + Line;
        int character = ColumnOffset + Column;
        var position = new TextPosition(line, character);
        var offset = Document.GetOffset(position);

        if (offset < Document.Text.Length - 1)
            Document.Replace(offset, 1, "");
    }

    public void DeletePreviousCharacter()
    {
        int line = LineOffset + Line;
        int character = ColumnOffset + Column;
        var position = new TextPosition(line, character);
        var offset = Document.GetOffset(position);

        int remove = 1;
        if (offset > 0 && Document.Text[offset - 1] == '\n')
        {
            int previousLineEnd = Document.GetLineLength(Line + LineOffset - 1);
            Document.Replace(offset - remove, remove, "");

            MoveUp();
            MoveHorizontal(previousLineEnd);
        }
        else
        {
            if (offset > 0)
            {
                Document.Replace(offset - 1, remove, "");

                MoveLeft();
            }
        }
    }

    public void InsertCharacter(string keySymbol)
    {
        int line = LineOffset + Line;
        int character = ColumnOffset + Column;
        var position = new TextPosition(line, character);
        var offset = Document.GetOffset(position);

        if (keySymbol[0] == '\r')
        {
            Document.Insert(offset, "\n");

            MoveDown();
            MoveToBeginningOfLine();
        }
        else
        {
            Document.Insert(offset, keySymbol);

            MoveRight();
        }
    }
}
