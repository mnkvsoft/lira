﻿using System.Text.Json.Serialization;

namespace Lira.Domain.Configuration.RangeModel.Dto;

record DataOptionsDto(
    [property: JsonPropertyName("type")]
    string Type,
    
    [property: JsonPropertyName("mode")]
    string? Mode,
   
    [property: JsonPropertyName("interval")]
    string? Interval,
    
    [property: JsonPropertyName("start")]
    string? Start,
    
    [property: JsonPropertyName("capacity")]
    string? Capacity,
    
    [property: JsonPropertyName("length")]
    string? Length,
    
    [property: JsonPropertyName("format")]
    string? Format,
    
    [property: JsonPropertyName("unit")]
    decimal? Unit,
    
    [property: JsonPropertyName("bytes_count")]
    int? BytesCount,
    
    [property: JsonPropertyName("ranges")]
    string[] Ranges);