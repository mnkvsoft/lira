﻿namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions.Generating.Impl.Create.Date;

internal class DateUtc : FunctionBase, IObjectTextPart
{
    public override string Name => "date.utc";
    
    public object Get(RequestData request)
    {
        var now = DateTime.UtcNow;
        var from = now.AddYears(-10);
        var ticks = Random.Shared.NextInt64(from.Ticks, now.Ticks);

        return new DateTime(ticks, DateTimeKind.Utc);
    }
}
