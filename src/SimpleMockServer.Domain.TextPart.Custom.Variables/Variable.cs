﻿using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Custom.Variables;

public record Variable : IObjectTextPart, IUniqueSetItem
{
    private readonly DeclaredItemName _name;
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;
    private static readonly object NullValue = new();

    public string Name => _name.Value;
    public string EntityName => "variable";
    
    public Variable(DeclaredItemName name, IReadOnlyCollection<IObjectTextPart> parts)
    {
        _parts = parts;
        _name = name;
    }

    public object? Get(RequestData request)
    {
        var key = "variable_" + _name;
        if (request.Items.TryGetValue(key, out var value))
        {
            if (value == NullValue)
                return null;
                
            return value;
        }

        object? newValue = _parts.Generate(request);
        request.Items.Add(key, newValue ?? NullValue);
        return newValue;
    }
}
