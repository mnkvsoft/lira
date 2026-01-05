using Lira.Domain.Handling.Generating.History;

namespace Lira.Domain.Handling.Generating.ResponseStrategies;

record WriteStatDependencies(
    HandledRuleHistoryStorage Storage,
    WriteHistoryMode WriteHistoryMode);