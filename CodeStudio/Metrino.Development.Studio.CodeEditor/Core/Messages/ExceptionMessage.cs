using CommunityToolkit.Mvvm.Messaging.Messages;
using System;

namespace Metrino.Development.Studio.Library.Messages;

public class ExceptionMessage : ValueChangedMessage<Exception>
{
    public ExceptionMessage(Exception value) : base(value)
    {
    }
}
