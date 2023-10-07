using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Lira.Common;
using Moq.Contrib.HttpClient;

namespace Lira.IntegrationTests;

public class AppMocks
{
    public Mock<HttpMessageHandler> HttpMessageHandler = new Mock<HttpMessageHandler>()
        .Apply(mock =>
        {
            mock
            .SetupAnyRequest()
            .ReturnsResponse(System.Net.HttpStatusCode.OK);
        });

    public IServiceCollection Configure(IServiceCollection services)
    {
        services.RemoveAll(typeof(ExternalCalling.Http.Configuration.IHttpMessageHandlerFactory));

        var mock = new Mock<ExternalCalling.Http.Configuration.IHttpMessageHandlerFactory>();
        mock
            .Setup(x => x.Create())
            .Returns(HttpMessageHandler.Object);

        services.AddSingleton(mock.Object);

        return services;
    }
}
