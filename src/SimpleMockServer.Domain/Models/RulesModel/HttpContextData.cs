using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain.Models.RulesModel;

public record HttpContextData(RequestData Request, HttpResponse Response);
