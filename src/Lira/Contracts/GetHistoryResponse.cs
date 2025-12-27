// ReSharper disable NotAccessedPositionalProperty.Global
namespace Lira.Contracts;

public record GetHistoryResponse(IReadOnlyList<RuleInvoke> Invokes);
public record RuleInvoke(DateTime Time, Request Request, Result Result);
public record Request(string Path, string Query, IReadOnlyDictionary<string, string> Headers);
public record Result(Response? Response, bool Fault);
public record Response(int Code, IReadOnlyDictionary<string, string?> Headers, string? Body);
