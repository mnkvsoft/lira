using Lira.Common;

namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions;

internal interface IWithRangeArgumentFunction : IWithArgument
{
    void SetArgument(object from, object to);
}
internal interface IWithRangeArgumentFunction<T> : IWithArgument where T : struct, IComparable<T>
{
    void SetArgument(Interval<T> interval);
}

abstract class WithRangeArgumentFunction<T> : IWithRangeArgumentFunction, IWithRangeArgumentFunction<T> where T : struct, IComparable<T>
{
   public abstract void SetArgument(Interval<T> argument);
   public void SetArgument(object from, object to)
   {
       SetArgument(new Interval<T>((T)from, (T)to));
   }

   abstract public bool ArgumentIsRequired { get; }
}

