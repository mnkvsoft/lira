using Lira.Common.Extensions;
using Lira.Domain.Configuration;
using RequestData = Lira.Domain.RequestData;

namespace Lira.Middlewares;

class RoutingMiddleware : IMiddleware
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly ILogger _logger;

    public RoutingMiddleware(
        ILoggerFactory loggerFactory,
        IConfigurationLoader configurationLoader)
    {
        _configurationLoader = configurationLoader;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/sys"))
        {
            await next(context);
            return;
        }

        try
        {
            await HandleRequest(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error occured");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(e.ToString());
        }
    }

    private async Task HandleRequest(HttpContext context)
    {
        var req = context.Request;
        req.EnableBuffering();

        var state = await _configurationLoader.GetState();
        if (state is ConfigurationState.Error error)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(error.Exception.GetMessagesChain());
            return;
        }

        var ok = (ConfigurationState.Ok)state;
        var request = new RequestData(req.Method, req.Path, req.QueryString, req.Headers, req.Query, req.Body, req.Protocol);
        await ok.RequestHandler.Handle(request, context.Response);
    }
}