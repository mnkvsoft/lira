using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public record HttpContextData(RuleExecutingContext RuleExecutingContext, HttpResponse Response);
