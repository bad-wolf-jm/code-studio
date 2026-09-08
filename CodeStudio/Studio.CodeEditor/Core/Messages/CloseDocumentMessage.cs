using CommunityToolkit.Mvvm.Messaging.Messages;
using Studio.Library.ViewModels;

namespace Studio.Library.Messages;

public class CloseDocumentMessage : ValueChangedMessage<DocumentViewModelBase>
{
    public CloseDocumentMessage(DocumentViewModelBase value) : base(value)
    {
    }
}
