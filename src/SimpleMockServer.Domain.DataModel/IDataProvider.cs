namespace SimpleMockServer.Domain.DataModel;

public interface IDataProvider
{
    Data GetData(DataName name);
    IReadOnlyCollection<Data> GetAll();
    Data? Find(DataName name);
}
