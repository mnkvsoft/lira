// ReSharper disable InconsistentNaming

using System.Globalization;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

public class DynamicObjectBaseMatch : DynamicObjectBase
{
    public DynamicObjectBaseMatch(Dependencies dependencies) : base(dependencies)
    {
    }

    // when matching data, you cannot change the data
    protected IReadonlyCache cache => Cache;

    protected bool range(string rangeName, object? value, Func<decimal, decimal>? change = null, double? multiply = null, double? divide = null)
    {
        if (value == null)
            return false;

        if(value is string str)
        {
            if(string.IsNullOrWhiteSpace(str))
                return false;

            if (decimal.TryParse(str, out decimal decValue))
            {
                if (change != null)
                {
                    return GetRange(rangeName).ValueIsBelong(change(decValue).ToString(CultureInfo.InvariantCulture));
                }
                else if (multiply != null)
                {
                    return GetRange(rangeName).ValueIsBelong((decValue * (decimal)multiply.Value).ToString(CultureInfo.InvariantCulture));
                }
                else if (divide != null)
                {
                    return GetRange(rangeName).ValueIsBelong((decValue / (decimal)divide.Value).ToString(CultureInfo.InvariantCulture));
                }
            }
            return GetRange(rangeName).ValueIsBelong(str);
        }

        return GetRange(rangeName).ValueIsBelong(value.ToString() ?? "");
    }
}
