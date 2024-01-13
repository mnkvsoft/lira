namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectBaseMatch : DynamicObjectBase
{
    public DynamicObjectBaseMatch(Dependencies dependencies) : base(dependencies)
    {
    }

    protected bool range(string rangeName, object value)
    {
        if (value == null)
            return false;

        if(value is string str && string.IsNullOrWhiteSpace(str))
            return false;

        bool isMatch = GetRange(rangeName).ValueIsBelong(value.ToString());

        return isMatch;
    }
}
