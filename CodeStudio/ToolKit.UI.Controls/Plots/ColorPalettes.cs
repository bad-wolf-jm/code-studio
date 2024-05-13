using Metrino.Development.Core;

namespace ToolKit.UI.Controls;

public class Category10
{
    public string Name { get; } = "Category 10";

    public string Description { get; } = "A set of 20 unque colors used in " +
        "many data visualization libraries such as Matplotlib, Vega, and Tableau";

    public Color[] Colors { get; } = HexColors.Select(Color.FromHex).ToArray();

    private static readonly string[] HexColors =
    {
        "#1f77b4", "#aec7e8", "#ff7f0e", "#ffbb78", "#2ca02c",
        "#98df8a", "#d62728", "#ff9896", "#9467bd", "#c5b0d5",
        "#8c564b", "#c49c94", "#e377c2", "#f7b6d2", "#7f7f7f",
        "#c7c7c7", "#bcbd22", "#dbdb8d", "#17becf", "#9edae5",
    };
}

public class Category20
{
    public string Name { get; } = "Category 20";

    public string Description { get; } = "A set of 20 unque colors used in " +
        "many data visualization libraries such as Matplotlib, Vega, and Tableau";

    public Color[] Colors { get; } = HexColors.Select(Color.FromHex).ToArray();

    private static readonly string[] HexColors =
    {
        "#1f77b4", "#aec7e8", "#ff7f0e", "#ffbb78", "#2ca02c",
        "#98df8a", "#d62728", "#ff9896", "#9467bd", "#c5b0d5",
        "#8c564b", "#c49c94", "#e377c2", "#f7b6d2", "#7f7f7f",
        "#c7c7c7", "#bcbd22", "#dbdb8d", "#17becf", "#9edae5",
    };
}


