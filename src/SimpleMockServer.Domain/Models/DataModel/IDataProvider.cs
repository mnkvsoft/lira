namespace SimpleMockServer.Domain.Models.DataModel;

public interface IDataProvider
{
    Data GetData(DataName name);
}