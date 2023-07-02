namespace SimpleMockServer.Domain.DataModel.DataImpls.Float;

public abstract class FloatDataRange : DataRange<float>
{
    protected FloatDataRange(DataName name) : base(name)
    {
    }

    public override bool TryParse(string str, out float value) => float.TryParse(str, out value);
}
