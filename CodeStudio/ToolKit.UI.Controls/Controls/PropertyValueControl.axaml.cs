using Avalonia;
using Avalonia.Controls.Primitives;

namespace ToolKit.UI.Controls;

public class PropertyValueControl : TemplatedControl
{
    public static readonly StyledProperty<string> PropertyNameProperty
        = AvaloniaProperty.Register<PropertyValueControl, string>(nameof(PropertyName));

    public string PropertyName
    {
        get => GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    public static readonly StyledProperty<string> PropertyValueProperty
        = AvaloniaProperty.Register<PropertyValueControl, string>(nameof(PropertyValue));

    public string PropertyValue
    {
        get => GetValue(PropertyValueProperty);
        set => SetValue(PropertyValueProperty, value);
    }
}
