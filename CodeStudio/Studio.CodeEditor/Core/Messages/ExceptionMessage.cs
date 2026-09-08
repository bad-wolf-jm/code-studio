using CommunityToolkit.Mvvm.Messaging.Messages;
using System;

namespace Studio.Library.Messages;

public class ExceptionMessage : ValueChangedMessage<Exception>
{
    public ExceptionMessage(Exception value) : base(value)
    {
    }
}
