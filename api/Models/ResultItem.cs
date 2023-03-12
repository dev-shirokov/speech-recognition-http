namespace api.Models;

public sealed record ResultItem
{
    public decimal Conf { get; set; }
    public decimal End { get; set; }
    public decimal Start { get; set; }
    public string? Word { get; set; }
}