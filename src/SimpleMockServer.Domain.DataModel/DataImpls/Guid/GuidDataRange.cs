namespace SimpleMockServer.Domain.DataModel.DataImpls.Guid;

public abstract class GuidDataRange : DataRange<System.Guid>
{
    protected GuidDataRange(DataName name) : base(name)
    {
    }

    public override bool TryParse(string str, out System.Guid value) => System.Guid.TryParse(str, out value);
}
