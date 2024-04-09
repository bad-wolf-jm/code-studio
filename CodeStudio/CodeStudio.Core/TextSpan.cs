namespace Metrino.Development.Core;

public struct TextSpan
{
    public TextSpan(bool highlight, int offset, int length)
    {
        Highlight = highlight;
        Offset = offset;
        Length = length;
    }

    public bool Highlight { get; set; }
    public int Offset { get; set; }
    public int Length { get; set; }
}
