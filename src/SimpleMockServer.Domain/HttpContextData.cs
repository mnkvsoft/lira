using Microsoft.AspNetCore.Http;

namespace SimpleMockServer.Domain;

public record HttpContextData(RequestData Request, HttpResponse Response);
