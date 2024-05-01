using System;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Material.Icons;

namespace Metrino.Development.Studio.Library.Controls;

public interface ICompletionData
{
    MaterialIconKind Icon { get; set; }

    string Name { get; set; }

    string Namespace { get; set; }

    string Description { get; set;  }

    /// <summary>
    /// Perform the completion.
    /// </summary>
    /// <param name="textArea">The text area on which completion is performed.</param>
    /// <param name="completionSegment">The text segment that was used by the completion window if
    /// the user types (segment between CompletionWindow.StartOffset and CompletionWindow.EndOffset).</param>
    /// <param name="insertionRequestEventArgs">The EventArgs used for the insertion request.
    /// These can be TextCompositionEventArgs, KeyEventArgs, MouseEventArgs, depending on how
    /// the insertion was triggered.</param>
    void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs);
}