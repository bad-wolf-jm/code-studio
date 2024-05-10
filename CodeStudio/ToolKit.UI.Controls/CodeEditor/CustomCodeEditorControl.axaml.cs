using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Metrino.Development.Core;
using System.Globalization;

namespace ToolKit.UI.Controls;

public class CustomCodeEditorControl : TemplatedControl
{
    public static readonly StyledProperty<ICodeDocument> DocumentProperty
        = AvaloniaProperty.Register<CustomCodeEditorControl, ICodeDocument>(nameof(Document));

    public ICodeDocument Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    ICodeDocumentView _documentView;

    public static readonly StyledProperty<IBrush> LineNumbersForegroundProperty
        = AvaloniaProperty.Register<CustomCodeEditorControl, IBrush>(nameof(LineNumbersForeground));

    public IBrush LineNumbersForeground
    {
        get => GetValue(LineNumbersForegroundProperty);
        set => SetValue(LineNumbersForegroundProperty, value);
    }

    public CustomCodeEditorControl()
    {
        //this.GetObservable(BoundsProperty).Subscribe(OnResize);

        FontFamilyProperty.Changed.AddClassHandler<CustomCodeEditorControl>(OnFontFamilyChanged);
        FontSizeProperty.Changed.AddClassHandler<CustomCodeEditorControl>(OnFontFamilyChanged);
        DocumentProperty.Changed.AddClassHandler<CustomCodeEditorControl>(OnDocumentChanged);

        KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown);
    }

    private void OnDocumentChanged(CustomCodeEditorControl control, AvaloniaPropertyChangedEventArgs args)
    {
        _documentView = new CodeDocumentView(args.NewValue as ICodeDocument);
        _documentView.Resize(Columns, Rows);
        InvalidateVisual();
    }

    double CharacterWidth = -1;
    double CharacterHeight = -1;
    int Columns = -1;
    int Rows = -1;

    int TextMarginLeft = 1;
    int TextMarginRight = 1;
    int LineNumberColumnWidth = 6;
    int ColumnMarker = 80;

    int FirstTextColumn => LineNumberColumnWidth + TextMarginLeft;
    int TextColumnCount => Columns - FirstTextColumn - TextMarginRight;

    int ColumnMarkerPosition => ColumnMarker + FirstTextColumn;

    double FirstTextColumnPosition => FirstTextColumn * CharacterWidth;

    double LineNumberColumnAbsoluteWidth => LineNumberColumnWidth * CharacterWidth;

    double LineNumberRenderingAbsoluteWidth => (LineNumberColumnWidth - 1) * CharacterWidth;

    double ColumnMarkerWidth => CharacterWidth * 0.75;

    double ColumnMarkerAbsolutePosition => ColumnMarkerPosition * CharacterWidth;

    private void OnKeyDown(TopLevel level, KeyEventArgs args)
    {
        switch (args.PhysicalKey)
        {
            case PhysicalKey.ArrowLeft:
                _documentView.MoveLeft();
                InvalidateVisual();
                return;
            case PhysicalKey.ArrowRight:
                _documentView.MoveRight();
                InvalidateVisual();
                return;
            case PhysicalKey.ArrowUp:
                _documentView.MoveUp();
                InvalidateVisual();
                return;
            case PhysicalKey.ArrowDown:
                _documentView.MoveDown();
                InvalidateVisual();
                return;
            case PhysicalKey.Delete:
                _documentView.DeleteCharacter();
                InvalidateVisual();
                return;
            case PhysicalKey.Backspace:
                _documentView.DeletePreviousCharacter();
                InvalidateVisual();
                return;
            case PhysicalKey.Tab:
                // TODO: tab to space conversion in doucment class
                _documentView.InsertCharacter("    ");
                InvalidateVisual();
                return;
        }

        if (args.KeySymbol != null)
        {
            _documentView.InsertCharacter(args.KeySymbol);
            InvalidateVisual();
        }
    }

    private void OnFontFamilyChanged(CustomCodeEditorControl control, AvaloniaPropertyChangedEventArgs args)
    {
        var text = "\u2560";
        var typeface = new Typeface(FontFamily, FontStyle.Normal, FontWeight.Normal);
        var textLayout = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, null);

        var size = textLayout?.Width;

        if (CharacterWidth != textLayout.Width || CharacterHeight != textLayout.Height)
        {
            CharacterWidth = textLayout.WidthIncludingTrailingWhitespace;
            CharacterHeight = textLayout.Height;

            OnResize(Bounds);
            InvalidateVisual();
        }
    }

    private void OnResize(Rect size)
    {
        // Ensure that full characters fit, even if it means a small gap at the end
        Columns = Convert.ToInt32(Math.Floor(size.Width / CharacterWidth));
        Rows = Convert.ToInt32(Math.Floor(size.Height / CharacterHeight));

        if (_documentView != null)
            _documentView.Resize(TextColumnCount, Rows);

        InvalidateVisual();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return base.ArrangeOverride(finalSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return availableSize;
    }

    private void PaintLastColumn(DrawingContext context)
    {
        IBrush lastColumnBackground = new SolidColorBrush(Colors.Gray, 0.5);

        context.FillRectangle(lastColumnBackground, new Rect(ColumnMarkerAbsolutePosition, 0, ColumnMarkerWidth, Bounds.Height));
    }

    private void PaintLineNumbers(DrawingContext context, Typeface textFormat)
    {
        IBrush lastColumnBackground = new SolidColorBrush(Colors.Gray, 0.15);

        context.FillRectangle(lastColumnBackground, new Rect(0, 0, LineNumberColumnAbsoluteWidth, Bounds.Height));

        double drawY = 0.0;
        var typeface = new Typeface(textFormat.FontFamily, FontStyle.Normal, FontWeight.Normal);
        IBrush? color = LineNumbersForeground;

        for (int i = 0; i < Rows; i++)
        {
            string lineNumber;
            double drawX;
            FormattedText textLayout;

            if (i + _documentView.LineOffset < Document.LineCount)
            {
                if (i == _documentView.Line)
                {
                    lineNumber = $"{i + _documentView.LineOffset + 1}";
                    textLayout = new FormattedText(lineNumber, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, color);
                    drawX = (CharacterWidth * 0.5);
                }
                else
                {
                    lineNumber = $"{Math.Abs(i - _documentView.Line)}";
                    textLayout = new FormattedText(lineNumber, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, color);
                    drawX = LineNumberRenderingAbsoluteWidth - textLayout.Width;
                }
            }
            else
            {
                lineNumber = "~";
                textLayout = new FormattedText(lineNumber, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, color);
                drawX = (CharacterWidth * 0.5);
            }

            context.DrawText(textLayout, new Point(drawX, drawY));
            drawY += textLayout.Height;
        }
    }

    struct HighlightFormat
    {
        public Typeface Typeface;
        public IBrush Foreground;
    }

    Dictionary<string, HighlightFormat> _highlightFormatCache = new Dictionary<string, HighlightFormat>();

    private void PaintTextLayer(DrawingContext context, List<TextSpan> spans, Typeface textFormat)
    {
        double drawY = 0.0;

        var typeface = new Typeface(textFormat.FontFamily, FontStyle.Normal, FontWeight.Normal);

        IBrush? color = Foreground;

        int lastLineToDisplay = Math.Min(Document.LineCount, _documentView.LineOffset + Rows);
        int columnOffset = _documentView.ColumnOffset;
        for (int i = _documentView.LineOffset; i < lastLineToDisplay; i++)
        {
            var line = Document.GetLine(i);
            if (line.Length <= columnOffset)
            {
                drawY += CharacterHeight;
                continue;
            }

            //line = line.Substring(columnOffset);
            //if (line.Length == 0)
            //{
            //    drawY += CharacterHeight;
            //    continue;
            //}

            // Figure out how much of the line we can display
            var lineIsTruncated = (line.Length - columnOffset) >= TextColumnCount;
            var textToDisplay = line.Substring(columnOffset, Math.Min(line.Length, TextColumnCount));
            var highlights = Document.GetHighlights(i, columnOffset, textToDisplay.Length);
            FormattedText textLayout;
            double lineHeight = 0.0;
            if (highlights.Length == 0)
            {
                textLayout = new FormattedText(textToDisplay, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, color);
                context.DrawText(textLayout, new Point(FirstTextColumnPosition, drawY));
                lineHeight = Math.Max(textLayout.Height, lineHeight);
            }
            else
            {
                int firstCharacterToDisplay = columnOffset;
                double drawX = FirstTextColumnPosition;

                for (int j = 0; j < highlights.Length; j++)
                {
                    var hl = highlights[j];

                    if(firstCharacterToDisplay < hl.Offset)
                    {
                        var textBeforeHighlight = line.Substring(firstCharacterToDisplay, hl.Offset - firstCharacterToDisplay);
                        textLayout = new FormattedText(textBeforeHighlight, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, color);
                        context.DrawText(textLayout, new Point(drawX, drawY));

                        drawX += textLayout.WidthIncludingTrailingWhitespace;
                        lineHeight = Math.Max(textLayout.Height, lineHeight);
                        firstCharacterToDisplay += textBeforeHighlight.Length;
                    }

                    // Draw the highlight
                    if (!_highlightFormatCache.ContainsKey(hl.Class))
                    {
                        var format = Highlight.CaptureFormats[hl.Class];
                        var highlightFormat = new HighlightFormat();
                        highlightFormat.Typeface = new Typeface(textFormat.FontFamily, format.Style, format.Weight);
                        var color0 = Metrino.Development.Core.Color.FromHex(format.Foreground);
                        var color1 = new Avalonia.Media.Color((byte)color0.A, (byte)color0.R, (byte)color0.G, (byte)color0.B);
                        highlightFormat.Foreground = new SolidColorBrush(color1);
                        _highlightFormatCache[hl.Class] = highlightFormat;
                    }

                    var typeFace = _highlightFormatCache[hl.Class];
                    var textToHighlight = line.Substring(hl.Offset, hl.Length);
                    textLayout = new FormattedText(textToHighlight, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace.Typeface, FontSize, typeFace.Foreground);
                    context.DrawText(textLayout, new Point(drawX, drawY));

                    drawX += textLayout.WidthIncludingTrailingWhitespace;
                    lineHeight = Math.Max(textLayout.Height, lineHeight);
                    firstCharacterToDisplay += hl.Length;
                }


                if (firstCharacterToDisplay < line.Length)
                {
                    var textBeforeHighlight = line.Substring(firstCharacterToDisplay, line.Length - firstCharacterToDisplay);
                    textLayout = new FormattedText(textBeforeHighlight, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, color);
                    context.DrawText(textLayout, new Point(drawX, drawY));

                    //drawX += textLayout.WidthIncludingTrailingWhitespace;
                    lineHeight = Math.Max(textLayout.Height, lineHeight);
                    //firstCharacterToDisplay += textBeforeHighlight.Length;
                }
                // Render the visible portion of the test that is left over.
            }


            if (lineIsTruncated)
            {
                // Display small ellipsis marker in the last column ifthe line has been truncated
                var ellipsis = new FormattedText("\uf141", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, color);
                context.DrawText(ellipsis, new Point((Columns - 1) * CharacterWidth, drawY));
            }

            drawY += lineHeight;
        }
    }

    public sealed override void Render(DrawingContext context)
    {
        var textFormat = new Typeface(FontFamily, FontStyle, FontWeight);

        PaintLineNumbers(context, textFormat);
        PaintLastColumn(context);
        PaintCurrentLine(context);
        PaintTextLayer(context, null, textFormat);
        PaintSelection(context);
        PaintCursor(context);
    }

    private void PaintCurrentLine(DrawingContext context)
    {
        double cursorX = 0.0;
        double cursorY = _documentView.Line * CharacterHeight;

        IBrush? color = new SolidColorBrush((Foreground as SolidColorBrush)?.Color ?? Colors.White, 0.1);

        context.FillRectangle(color, new Rect(cursorX, cursorY, Bounds.Width, CharacterHeight));
    }

    private void PaintCursor(DrawingContext context)
    {
        double cursorX = (_documentView.Column + FirstTextColumn) * CharacterWidth;
        double cursorY = _documentView.Line * CharacterHeight;

        IBrush? color = Foreground;

        context.FillRectangle(color, new Rect(cursorX, cursorY, CharacterWidth, CharacterHeight));
    }

    private void PaintSelection(DrawingContext context)
    {
        //throw new NotImplementedException();
    }
}
