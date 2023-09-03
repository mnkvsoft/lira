using System.Text.Json.Serialization;

namespace SimpleMockServer.Domain.Configuration.DataModel.Dto;

record DataOptionsDto(
    [property: JsonPropertyName("type")]
    string Type,
    
    [property: JsonPropertyName("mode")]
    string? Mode,
    
    [property: JsonPropertyName("capacity")]
    string? Capacity,
    
    [property: JsonPropertyName("format")]
    string? Format,
    
    [property: JsonPropertyName("interval")]
    string? Interval,
    
    [property: JsonPropertyName("unit")]
    decimal? Unit,
    
    [property: JsonPropertyName("bytes_count")]
    int? BytesCount,
    
    [property: JsonPropertyName("ranges")]
    string[] Ranges);