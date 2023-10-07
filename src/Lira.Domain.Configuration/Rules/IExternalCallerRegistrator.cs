using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.FileSectionFormat;

namespace Lira.Domain.Configuration.Rules;

public interface IExternalCallerRegistrator
{
    string Name { get; }

    Task<IExternalCaller> Create(FileSection section, IParsingContext parsingContext);
}
