using System.Text.Json.Serialization;

namespace Lira.Domain.Configuration.RangeModel.Dto;

record DataOptionsDto(
    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("description")]
    string? Description,

    [property: JsonPropertyName("mode")]
    string? Mode,

    [property: JsonPropertyName("interval")]
    string? Interval,

    [property: JsonPropertyName("start")]
    string? Start,

    [property: JsonPropertyName("capacity")]
    string? Capacity,

    [property: JsonPropertyName("format")]
    string? Format,

    [property: JsonPropertyName("unit")]
    decimal? Unit,

    [property: JsonPropertyName("bytes_count")]
    int? BytesCount,

    [property: JsonPropertyName("ranges")]
    string[] Ranges,

    [property: JsonPropertyName("bins")]
    int[]? Bins);