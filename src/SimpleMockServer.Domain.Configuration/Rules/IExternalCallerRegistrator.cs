using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules;

public interface IExternalCallerRegistrator
{
    string Name { get; }

    Task<IExternalCaller> Create(FileSection section, IParsingContext parsingContext);
}
