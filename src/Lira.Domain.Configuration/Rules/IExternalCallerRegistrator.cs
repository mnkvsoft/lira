﻿using Lira.Domain.Actions;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules;

public interface ISystemActionRegistrator
{
    string Name { get; }

    Task<IAction> Create(FileSection section, IParsingContext parsingContext);
}
