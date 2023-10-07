using Microsoft.AspNetCore.Http;

namespace Lira.Domain;

public record HttpContextData(RequestData Request, HttpResponse Response);
