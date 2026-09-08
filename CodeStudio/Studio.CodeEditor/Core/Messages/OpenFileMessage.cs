using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Studio.Library.Messages;

public class OpenFileMessage : ValueChangedMessage<string>
{
    public OpenFileMessage(string value) : base(value)
    {
    }
}
