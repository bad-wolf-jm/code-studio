using Avalonia;
using Avalonia.Controls.Primitives;
using AvaloniaEdit.Document;

namespace Metrino.Development.Studio.Library.Controls;

public class CodeSnippet : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty
        = AvaloniaProperty.Register<CodeEditorControl, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> DescriptionProperty
        = AvaloniaProperty.Register<CodeEditorControl, string>(nameof(Description));

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<string> LanguageProperty
        = AvaloniaProperty.Register<CodeEditorControl, string>(nameof(Language));

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    public static readonly StyledProperty<TextDocument> DocumentProperty
        = AvaloniaProperty.Register<CodeEditorControl, TextDocument>(nameof(Document));

    public TextDocument Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }
}