//using Microsoft.AspNetCore.Http;
//using Lira.Domain.Models.RulesModel;

//namespace Lira.Domain.Extensions;

//internal static class ResponseDataExtensions
//{
//    public static async Task WriteTo(this ResponseData data, HttpResponse response)
//    {
//        if (data.Code == null)
//            throw new Exception("Http code must be set");

//        response.StatusCode = data.Code.Value;
//        AddHeaders(data, response);

//        await response.WriteAsync(data.Body.ToString());
//    }

//    private static void AddHeaders(ResponseData data, HttpResponse response)
//    {
//        foreach (var header in data.Headers)
//        {
//            response.Headers.Add(header.Key, header.Value);
//        }
//    }
//}