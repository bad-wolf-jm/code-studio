using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Metrino.Development.Studio.Library.Messages;

public class OpenFileMessage : ValueChangedMessage<string>
{
    public OpenFileMessage(string value) : base(value)
    {
    }
}
