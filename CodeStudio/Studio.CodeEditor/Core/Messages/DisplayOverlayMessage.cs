using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Studio.Library.Messages;

public class DisplayOverlayMessage : ValueChangedMessage<object>
{
    public DisplayOverlayMessage(object value) : base(value)
    {
    }
}

public class DismissOverlayMessage : ValueChangedMessage<object>
{
    public DismissOverlayMessage(object value) : base(value)
    {
    }
}
