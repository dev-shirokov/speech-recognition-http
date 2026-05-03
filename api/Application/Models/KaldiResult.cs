using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace api.Application.Models;

public sealed record KaldiResult
{
    [JsonProperty("result")]
    public ResultItem[]? Items { get; set; }

    public string? Text { get; set; }
    public string? Partial { get; set; }
}
public sealed record ResultItem
{
    public decimal Conf { get; set; }
    public decimal End { get; set; }
    public decimal Start { get; set; }
    public string? Word { get; set; }
}
