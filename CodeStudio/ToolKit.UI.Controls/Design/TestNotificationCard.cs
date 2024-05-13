using Avalonia.Controls.Notifications;

namespace Metrino.Development.Studio.Design;

public class TestErrorNotificationCard : INotification
{
    public string? Title => "Title";
    public string? Message => "Message";
    public NotificationType Type => NotificationType.Error;
    public TimeSpan Expiration => TimeSpan.FromMicroseconds(0);
    public Action? OnClick => null;
    public Action? OnClose => null;
}

public class TestWarningNotificationCard : INotification
{
    public string? Title => "Title";
    public string? Message => "Message";
    public NotificationType Type => NotificationType.Warning;
    public TimeSpan Expiration => TimeSpan.FromMicroseconds(0);
    public Action? OnClick => null;
    public Action? OnClose => null;
}

public class TestInfoNotificationCard : INotification
{
    public string? Title => "Title";
    public string? Message => "Message";
    public NotificationType Type => NotificationType.Information;
    public TimeSpan Expiration => TimeSpan.FromMicroseconds(0);
    public Action? OnClick => null;
    public Action? OnClose => null;
}

public class TestSuccessNotificationCard : INotification
{
    public string? Title => "Title";
    public string? Message => "Message";
    public NotificationType Type => NotificationType.Success;
    public TimeSpan Expiration => TimeSpan.FromMicroseconds(0);
    public Action? OnClick => null;
    public Action? OnClose => null;
}
