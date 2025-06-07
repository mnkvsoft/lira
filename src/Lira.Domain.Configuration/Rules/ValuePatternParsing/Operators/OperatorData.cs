using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

record OperatorData(
    string Name,
    string Parameters,
    IReadOnlyCollection<IObjectTextPart> Body,
    IReadOnlyCollection<OperatorDataItem> Items);

record OperatorDataItem(string Name, string Parameters, IReadOnlyCollection<IObjectTextPart> Body);