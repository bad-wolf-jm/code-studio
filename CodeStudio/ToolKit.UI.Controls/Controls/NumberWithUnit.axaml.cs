using Avalonia;
using Avalonia.Controls.Primitives;

namespace ToolKit.UI.Controls;

public class NumberWithUnit : TemplatedControl
{
    public static readonly StyledProperty<string> ValueProperty
        = AvaloniaProperty.Register<NumberWithUnit, string>(nameof(Value));

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<string> UnitProperty
        = AvaloniaProperty.Register<NumberWithUnit, string>(nameof(Unit));

    public string Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

}