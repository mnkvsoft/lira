using Lira.Domain.TextPart;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators;

record OperatorDraft(string Name, string Parameters, List<IObjectTextPart> Content);