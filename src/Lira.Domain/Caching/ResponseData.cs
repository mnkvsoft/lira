namespace Lira.Domain.Caching;

public record ResponseData(
    int Code,
    IReadOnlyCollection<string>? BodyParts,
    IReadOnlyCollection<Header>? Headers);