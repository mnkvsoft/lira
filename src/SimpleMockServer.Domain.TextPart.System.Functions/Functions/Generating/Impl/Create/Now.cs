using Microsoft.Extensions.Configuration;

namespace SimpleMockServer.Domain.TextPart.System.Functions.Functions.Generating.Impl.Create;

internal class Now : IObjectTextPart
{
    public static string Name => "now";
    
    private readonly TimeSpan _timezoneOffset;

    public Now(IConfiguration configuration)
    {
        // Because Alpina image has trouble setting the local time, so
        
        var offsetStr = configuration.GetValue<string>("TimezoneOffset");
        
        if (string.IsNullOrWhiteSpace(offsetStr))
            throw new Exception("Not defined TimezoneOffset in configuration");
        
        var arr = offsetStr[1..].Split(":");
        int hours = int.Parse(arr[0]);
        int minutes = int.Parse(arr[1]);

        _timezoneOffset = new TimeSpan(0, hours, minutes, 0);
        if (offsetStr[0] == '-')
            _timezoneOffset = -_timezoneOffset;
    }

    public object Get(RequestData request) => DateTime.UtcNow.Add(_timezoneOffset);
}
