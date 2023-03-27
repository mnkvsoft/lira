using Microsoft.AspNetCore.Http;
using SimpleMockServer.Domain.Models.DataModel;

namespace SimpleMockServer.Domain.Functions.Pretty.Functions.Generating.Impl;
internal class Data : DataBase, IGeneratingPrettyFunction
{
    public Data(IDataProvider dataProvider) : base(dataProvider)
    {
    }

    public static string Name => "data";

    public object? Generate(HttpRequest request)
    {
        return GetRange().NextValue().ToString();
    }
}
