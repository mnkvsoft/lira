﻿namespace Lira.Domain.TextPart.Impl.CSharp;

public interface IDeclaredPartsProvider
{
    IObjectTextPart Get(string name);

    ReturnType? GetPartType(string name);

    void SetVariable(string name, RuleExecutingContext context, dynamic value);
}