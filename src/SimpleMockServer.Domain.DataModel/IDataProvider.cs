namespace SimpleMockServer.Domain.DataModel;

public interface IDataProvider
{
    Data GetData(DataName name);
}
