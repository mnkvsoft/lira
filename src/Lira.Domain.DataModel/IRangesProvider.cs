namespace Lira.Domain.DataModel;

public interface IRangesProvider
{
    Data Get(DataName name);
    IReadOnlyCollection<Data> GetAll();
    Data? Find(DataName name);
}
