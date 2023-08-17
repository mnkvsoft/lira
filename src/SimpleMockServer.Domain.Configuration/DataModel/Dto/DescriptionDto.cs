using System.Text.Json.Serialization;

namespace SimpleMockServer.Domain.Configuration.DataModel.Dto;

record DataOptionsDto(
    [property: JsonPropertyName("type")]
    string Type,
    
    [property: JsonPropertyName("mode")]
    string? Mode,
    
    [property: JsonPropertyName("capacity")]
    string? Capacity,
    
    [property: JsonPropertyName("interval")]
    string? Interval,
    
    [property: JsonPropertyName("unit")]
    decimal? Unit,
    
    [property: JsonPropertyName("bytes_count")]
    int? BytesCount,
    
    // todo: проверить будет ли исключение в случае дубликата
    [property: JsonPropertyName("ranges")]
    string[] Ranges);