using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace api.Models;

public sealed record KaldiResult
{
    [JsonProperty("result")]
    public ResultItem[]? Items { get; set; }

    public string? Text { get; set; }
    public string? Partial { get; set; }
}
