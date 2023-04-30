using SimpleMockServer.Domain.Models.RulesModel;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.ConfigurationProviding.Rules;

public interface IExternalCallerRegistrator
{
    string Name { get; }

    IReadOnlyCollection<string> GetSectionKnowsBlocks();

    IExternalCaller Create(FileSection section, VariableSet variables);
}
