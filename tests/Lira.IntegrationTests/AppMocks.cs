using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Lira.Common;
using Lira.Common.Extensions;
using Lira.Domain.Configuration;
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

    public Mock<IStateRepository> StateRepository = new Mock<IStateRepository>().Apply(r =>
    {
        r.Setup(x => x.GetStates())
            .ReturnsAsync(ImmutableDictionary<string, string>.Empty);
    });

    public IServiceCollection Configure(IServiceCollection services)
    {
        var claims = new List<Claim>
        {
            new("phone", "9161112233"),
        };


        var mock = new Mock<ExternalCalling.Http.Configuration.IHttpMessageHandlerFactory>();
        mock
            .Setup(x => x.Create())
            .Returns(HttpMessageHandler.Object);

        services.Replace(mock.Object);
        services.Replace(StateRepository.Object);

        return services;
    }
}