using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel;

public record HttpContextData(HttpRequest Request, HttpResponse Response);