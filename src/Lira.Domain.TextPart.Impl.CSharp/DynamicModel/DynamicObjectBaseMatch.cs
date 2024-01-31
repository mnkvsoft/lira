namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectBaseMatch : DynamicObjectBase
{
    public DynamicObjectBaseMatch(Dependencies dependencies) : base(dependencies)
    {
    }

    // when matching data, you cannot change the data
    protected IReadonlyCache cache => _cache;

    protected bool range(string rangeName, object value, double? multiply = null, double? divide = null)
    {
        if (value == null)
            return false;

        if(value is string str)
        { 
            if(string.IsNullOrWhiteSpace(str))
                return false;

            if (decimal.TryParse(str, out decimal decValue))
            {
                if (multiply != null)
                {
                    return GetRange(rangeName).ValueIsBelong((decValue * (decimal)multiply.Value).ToString());
                }
                else if (divide != null)
                {
                    return GetRange(rangeName).ValueIsBelong((decValue / (decimal)divide.Value).ToString());
                }
            }
            return GetRange(rangeName).ValueIsBelong(str);
        }

        return GetRange(rangeName).ValueIsBelong(value.ToString() ?? "");
    }
}
