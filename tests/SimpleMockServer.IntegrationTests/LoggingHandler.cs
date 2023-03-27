using SimpleMockServer.IntegrationTests.Extensions;

namespace SimpleMockServer.IntegrationTests;

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Request:");
        Console.WriteLine($"{request.Method} {request.RequestUri}");
        Console.WriteLine();

        foreach (var header in request.Headers)
        {
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        if (request.Content != null)
        {
            Console.WriteLine(await request.Content.ReadAsStringAsync());
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        Console.WriteLine();
        Console.WriteLine("Response:");
        Console.WriteLine((int)response.StatusCode);
        
        var headers = response.GetAllHeaders();
        if (headers.Any())
        {
            Console.WriteLine();
            foreach (var header in headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        if (response.Content != null)
        {
            Console.WriteLine();
            string value = await response.Content.ReadAsStringAsync();
            Console.WriteLine(value);
        }

        Console.WriteLine("--------------------------------------------------------------------------------");

        return response;
    }
}
