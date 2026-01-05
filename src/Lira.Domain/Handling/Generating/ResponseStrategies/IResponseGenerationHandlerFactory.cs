using Lira.Domain.Handling.Generating.History;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Fault;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal;
using Lira.Domain.Handling.Generating.ResponseStrategies.Impl.Normal.Generators;

namespace Lira.Domain.Handling.Generating.ResponseStrategies;

public interface IResponseGenerationHandlerFactory
{
    IHandler CreateNormal(
        WriteHistoryMode writeHistoryMode,
        IHttCodeGenerator codeGenerator,
        BodyGenerator? bodyGenerator,
        HeadersGenerator? headersGenerator);

    IHandler CreateFault(WriteHistoryMode writeHistoryMode);
}

class ResponseGenerationHandlerFactory(HandledRuleHistoryStorage handledRuleHistoryStorage) : IResponseGenerationHandlerFactory
{
    public IHandler CreateNormal(
        WriteHistoryMode writeHistoryMode,
        IHttCodeGenerator codeGenerator,
        BodyGenerator? bodyGenerator,
        HeadersGenerator? headersGenerator)
    {
        return new NormalResponseGenerationHandler(
            new WriteStatDependencies(handledRuleHistoryStorage, writeHistoryMode),
            codeGenerator,
            bodyGenerator,
            headersGenerator);
    }

    public IHandler CreateFault(WriteHistoryMode writeHistoryMode)
    {
        return new FaultResponseGenerationHandler(
            new WriteStatDependencies(handledRuleHistoryStorage, writeHistoryMode));
    }
}