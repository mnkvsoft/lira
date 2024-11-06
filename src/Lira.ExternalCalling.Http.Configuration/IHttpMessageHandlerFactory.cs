namespace Lira.ExternalCalling.Http.Configuration;

public interface IHttpMessageHandlerFactory
{
    HttpMessageHandler Create();
}

class HttpMessageHandlerFactory : IHttpMessageHandlerFactory
{
    public HttpMessageHandler Create()
    {
        return new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
    }
}
