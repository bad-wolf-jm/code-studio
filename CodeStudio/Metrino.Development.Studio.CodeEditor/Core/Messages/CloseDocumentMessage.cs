using CommunityToolkit.Mvvm.Messaging.Messages;
using Metrino.Development.Studio.Library.ViewModels;

namespace Metrino.Development.Studio.Library.Messages;

public class CloseDocumentMessage : ValueChangedMessage<DocumentViewModelBase>
{
    public CloseDocumentMessage(DocumentViewModelBase value) : base(value)
    {
    }
}
