﻿using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.Impl.Custom;

public record Function : IObjectTextPart, IUniqueSetItem
{
    private readonly IReadOnlyCollection<IObjectTextPart> _parts;


    private readonly CustomItemName _name;
    public string Name => _name.Value;
    public string EntityName => "function";
    
    public Function(CustomItemName name, IReadOnlyCollection<IObjectTextPart> parts)
    {
        _parts = parts;
        _name = name;
    }

    public object? Get(RequestData request) => _parts.Generate(request);
}