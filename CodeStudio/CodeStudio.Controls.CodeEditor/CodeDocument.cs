using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metrino.Development.Core.TreeSitter;
using Avalonia.Platform;
using System.Diagnostics;
//using System.Windows.Media;

namespace Metrino.Development.Studio.Library.Controls;

public struct TextHighlight
{
    public string Class = string.Empty;
    public int Offset = 0;
    public int Length = 0;

    public TextHighlight()
    {
    }
}

public struct TextLine
{
    public int Offset;
    public int Length;

    public List<TextHighlight> Highlights;

    public TextLine(int offset, int length) : this()
    {
        Offset = offset;
        Length = length;
        Highlights = new List<TextHighlight>();
    }
}

public partial class CodeDocument : ObservableObject, ICodeDocument
{
    [ObservableProperty]
    string _text = string.Empty;

    TSParser _parser;
    TSTree _tree;
    TSQuery? _highlightQuery;

    public List<TextLine> Lines { get; set; }
    List<TextHighlight>? _highlights;

    public void Load(string path)
    {
        // Internally use "\n" as line ending
        Text = File.ReadAllText(path).ReplaceLineEndings("\n");
        _parser = TSParser.FromLanguageName("lua");
        _highlights = new List<TextHighlight>();

        var uri = $"avares://Metrino.Development.Studio.Library/Assets/SyntaxHighlights/lua/highlights.scm";
        using (var query = AssetLoader.Open(new System.Uri(uri)))
        using (var sr = new StreamReader(query))
        {
            var q = sr.ReadToEnd();
            uint errorOffset;
            ETSQueryError errorType;
            _highlightQuery = _parser.Language().QueryNew(q, out errorOffset, out errorType);
            if (errorType != ETSQueryError.TSQueryErrorNone)
            {
                var problem = q.Substring((int)errorOffset);
                _highlightQuery = null;
            }
        }

        UpdateLines();
        //ParseDocument();
    }

    private void ParseDocument()
    {
        _tree = _parser.ParseString(null, Text);

        if (_highlightQuery == null)
            return;


        TSNode rootNode = _tree.RootNode;
        TSQueryCursor queryCursor = new TSQueryCursor();
        queryCursor.SetRange(0, (uint)Text.Length);
        queryCursor.Execute(_highlightQuery, rootNode);

        TSQueryMatch match;
        TSQueryCapture[] captures;
        Highlight _hl = new Highlight();
        //var highlights = new List<TextHighlight>();
        _highlights.Clear();

        var hasMatch = queryCursor.NextMatch(out match, out captures);
        while (hasMatch)
        {
            var matchPredicate = _highlightQuery.Predicates[match.PatternIndex];
            //foreach (var pred in matchPredicate)
            if (!matchPredicate.Test(_highlightQuery, captures, Text))
            {
                hasMatch = queryCursor.NextMatch(out match, out captures);
                continue;
            }

            foreach (var capture in captures)
            {
                uint length;
                var name = _highlightQuery.CaptureNameForId(capture.Index, out length);
                if (!_hl.CaptureNames.Contains(name))
                    continue;

                var captureStartOffset = (int)capture.Node.StartOffset;
                var captureEndOffset = (int)capture.Node.EndOffset;
                TextHighlight? hl = null;
                foreach (var h in _highlights)
                {
                    if (h.Offset <= captureStartOffset && h.Offset + h.Length >= captureEndOffset)
                    {
                        hl = h;
                        break;
                    }
                }

                if (hl == null)
                {
                    _highlights.Add(new TextHighlight
                    {
                        Class = name,
                        Offset = (int)capture.Node.StartOffset - 0,
                        Length = (int)capture.Node.EndOffset - (int)capture.Node.StartOffset
                    });

                    var T = Text.Substring((int)capture.Node.StartOffset, (int)capture.Node.EndOffset - (int)capture.Node.StartOffset);
                    //Debug.WriteLine($"  ** {name} - {(int)capture.Node.StartOffset - 0} - {(int)capture.Node.EndOffset - (int)capture.Node.StartOffset} - {T}");
                }
            }

            hasMatch = queryCursor.NextMatch(out match, out captures);
        }

        foreach (var line in Lines)
        {
            //    Debug.WriteLine(Text.Substring(line.Offset, line.Length));

            //    //Debug.WriteLine($"");
        }
    }

    public void UpdateLines()
    {
        Lines = new List<TextLine>();
        int currentOffset = 0;
        int currentLength = 0;

        for (int i = 0; i < Text.Length; i++)
        {
            if (Text[i] == '\n')
            {
                // Internally the end-of-line character is ignored so that the length of the
                // line reflects the number of characters actually displayed.
                Lines.Add(new TextLine(currentOffset, currentLength));

                currentLength = 0;
                currentOffset = i + 1;
            }
            else
            {
                currentLength++;
            }
        }

        ParseDocument();
    }

    public int LineCount => Lines?.Count() ?? 0;

    public string GetLine(int i)
        => Text.Substring(Lines.ElementAt(i).Offset, Lines.ElementAt(i).Length);


    public void Replace(int offset, int length, string @string)
    {
        if (length > 0)
            Text = Text.Remove(offset, length);

        if (!string.IsNullOrEmpty(@string))
            Text = Text.Insert(offset, @string);

        UpdateLines();
    }

    public void Replace(TextPosition position, int length, string @string)
        => Replace(GetOffset(position), length, @string);


    public int GetOffset(TextPosition position)
    {
        var line = Lines[position.Line];

        return line.Offset + position.Column;
    }

    public TextPosition GetPosition(int offset)
    {
        int lineIndex = 0;
        foreach (var line in Lines)
        {
            if (offset >= line.Offset && offset < line.Length + line.Offset)
                return new TextPosition(lineIndex, offset - line.Offset);

            lineIndex++;
        }

        return new TextPosition(Lines.Count, 0);
    }

    public void Insert(int offset, string @string)
        => Replace(offset, 0, @string);


    public void Insert(TextPosition position, string @string)
        => Insert(GetOffset(position), @string);


    public int GetLineLength(int i)
    {
        if (i >= 0 && i < Lines.Count)
            return Lines[i].Length;

        return 0;
    }

    public TextHighlight[] GetHighlights(int line, int columnOffset, int length)
    {
        //return Lines[line].Highlights
        //    .Select(x => new TextHighlight { Class = x.Class, Offset = x.Offset, Length = x.Length })
        //    .Where(x => x.Offset >= 0 && x.Offset < length)
        //    .ToArray();
        return _highlights
            .Where(x => x.Offset >= Lines[line].Offset && x.Offset + x.Length <= Lines[line].Offset + Lines[line].Length)
            .Select(x => new TextHighlight { Class = x.Class, Offset = x.Offset - Lines[line].Offset, Length = x.Length })
            .ToArray();


    }
}
