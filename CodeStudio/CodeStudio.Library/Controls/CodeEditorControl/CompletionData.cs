using System;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Material.Icons;

namespace Metrino.Development.Studio.Library.Controls;

public class CompletionData : ICompletionData
{
    public MaterialIconKind Icon { get; set; }

    public string Name { get; set; }

    public string Namespace { get; set; }

    public string Description { get; set; }

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Name);
    }
}
