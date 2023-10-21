﻿using Lira.Domain.Extensions;
using Lira.Domain.Matching.Request.Matchers;
using Lira.Domain.TextPart.Utils;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Extract.Body;

class JsonPathExtractFunction : WithArgumentFunction<string>, IBodyExtractFunction, IObjectTextPart
{
    public static string Name => "req.body.jpath";
    public override bool ArgumentIsRequired => true;

    private string _jpath = "";

    public string? Extract(string? body) => BodyUtils.GetByJPath(body, _jpath);

    public object? Get(RequestData request) => Extract(request.ReadBody());

    public override void SetArgument(string argument) => _jpath = argument;
}
