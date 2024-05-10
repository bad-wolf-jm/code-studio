using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Metrino.Development.Core;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ToolKit.UI.Controls;

public class TextHighlightControl : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty
        = AvaloniaProperty.Register<TextHighlightControl, string>(nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<List<TextSpan>> TextSpansProperty
        = AvaloniaProperty.Register<TextHighlightControl, List<TextSpan>>(nameof(TextSpans));

    public List<TextSpan> TextSpans
    {
        get => GetValue(TextSpansProperty);
        set => SetValue(TextSpansProperty, value);
    }

    public static readonly StyledProperty<IBrush> HighlightColorProperty
        = AvaloniaProperty.Register<TextHighlightControl, IBrush>(nameof(HighlightColor));

    public IBrush HighlightColor
    {
        get => GetValue(HighlightColorProperty);
        set => SetValue(HighlightColorProperty, value);
    }

    public TextHighlightControl()
    {
        PropertyChanged += TextHighlightControl_PropertyChanged;
        TextSpans = new List<TextSpan>() { new TextSpan(false, 0, 0) };
    }

    private void TextHighlightControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextProperty || e.Property == TextSpansProperty)
        {
            InvalidateVisual();
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return base.ArrangeOverride(finalSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var textFormat = new Typeface(FontFamily, FontStyle, FontWeight);
        double width = 0.0;
        double height = 0.0;

        foreach (var textSpan in TextSpans)
        {
            if (textSpan.Length == 0)
                continue;

            var text = Text.Substring(textSpan.Offset, textSpan.Length);
            var typeface = new Typeface(textFormat.FontFamily, FontStyle.Normal, textSpan.Highlight ? FontWeight.Bold : FontWeight.Normal);
            var textLayout = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, null);

            width += textLayout.WidthIncludingTrailingWhitespace;
            height = Math.Max(height, textLayout.Height);
        }

        return new Size(width, height);
    }

    private void PaintTextLayer(DrawingContext context, List<TextSpan> spans, Typeface textFormat)
    {
        double drawX = 0.0;
        double drawY = 0.0;

        if (Background != null)
            context.FillRectangle(Background, Bounds);

        foreach (var textSpan in TextSpans)
        {
            if (textSpan.Length == 0)
                continue;

            var text = Text.Substring(textSpan.Offset, textSpan.Length);
            var typeface = new Typeface(textFormat.FontFamily, FontStyle.Normal, textSpan.Highlight ? FontWeight.Bold : FontWeight.Normal);

            IBrush? color = Foreground;

            if (textSpan.Highlight)
            {
                color = HighlightColor;
            }

            var textLayout = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, color);

            context.DrawText(textLayout, new Point(drawX, drawY));

            drawX += textLayout.WidthIncludingTrailingWhitespace;
        }
    }

    public sealed override void Render(DrawingContext context)
    {
        //
        var textFormat = new Typeface(FontFamily, FontStyle, FontWeight);

        PaintTextLayer(context, TextSpans, textFormat);
    }
}
