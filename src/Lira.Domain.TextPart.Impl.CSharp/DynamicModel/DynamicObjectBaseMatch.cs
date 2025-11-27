// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System.Globalization;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

// ReSharper disable once UnusedType.Global
public abstract class DynamicObjectBaseMatch : DynamicObjectWithDeclaredPartsBase
{
    protected DynamicObjectBaseMatch(DependenciesBase dependencies) : base(dependencies)
    {
    }

    // when matching data, you cannot change the data
    protected IReadonlyCache cache => Cache;

    protected bool range(string rangeName, object? value, Func<double, double>? change = null, double? multiply = null, double? divide = null)
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
                    return GetRange(rangeName).ValueIsBelong(change((double)decValue).ToString(CultureInfo.InvariantCulture));

                if (multiply != null)
                    return GetRange(rangeName).ValueIsBelong((decValue * (decimal)multiply.Value).ToString(CultureInfo.InvariantCulture));

                if (divide != null)
                    return GetRange(rangeName).ValueIsBelong((decValue / (decimal)divide.Value).ToString(CultureInfo.InvariantCulture));
            }
            return GetRange(rangeName).ValueIsBelong(str);
        }

        return GetRange(rangeName).ValueIsBelong(value.ToString() ?? "");
    }

    protected bool dic(string dicName, object? value)
    {
        if (value == null)
            return false;

        if(value is string str)
        {
            return GetDic(dicName).ValueIsBelong(str);
        }

        return GetRange(dicName).ValueIsBelong(value.ToString() ?? "");
    }
}
