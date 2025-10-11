using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Lira.Common.Extensions;
using Lira.Domain.Configuration;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Moq.Contrib.HttpClient;

namespace Lira.IntegrationTests;

class AppMocks
{
    public PeImagesCache? PeImagesCache { get; set; }

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
        var mock = new Mock<ExternalCalling.Http.Configuration.IHttpMessageHandlerFactory>();
        mock
            .Setup(x => x.Create())
            .Returns(HttpMessageHandler.Object);

        services.Replace(mock.Object);
        services.Replace(StateRepository.Object);

        if(PeImagesCache != null)
            services.Replace(PeImagesCache);

        return services;
    }
}