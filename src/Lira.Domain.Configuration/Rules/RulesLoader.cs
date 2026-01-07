using Lira.Domain.Configuration.Rules.ValuePatternParsing;

namespace Lira.Domain.Configuration.Rules;

internal class RulesLoader(RuleFileLoader ruleFileLoader)
{
    public async Task LoadRulesDatas(
        IRequestHandlerBuilder builder,
        string path,
        IReadonlyParsingContext parsingContext)
    {
        var rulesFiles = DirectoryHelper.GetFiles(path, "*.rules");

        foreach (var ruleFile in rulesFiles)
        {
            try
            {
                await ruleFileLoader.Load(builder, ruleFile, parsingContext);
            }
            catch (Exception exc)
            {
                throw new FileParsingException(ruleFile, exc);
            }
        }
    }
}
