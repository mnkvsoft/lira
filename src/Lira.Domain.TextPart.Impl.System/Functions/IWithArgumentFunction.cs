namespace Lira.Domain.TextPart.Impl.System.Functions;

// todo: IWithRangeArgumentFunction, IWithArgumentFunction combine with IWithArgument
internal interface IWithArgumentFunction : IWithArgument
{
    void SetArgument(object argument);
}

internal interface IWithArgumentFunction<in T> : IWithArgument
{
    void SetArgument(T arguments);
}

internal abstract class WithArgumentFunction<T> : FunctionBase, IWithArgumentFunction, IWithArgumentFunction<T>
{
     public abstract void SetArgument(T arguments);
     public void SetArgument(object argument)
     {
         SetArgument((T)argument);
     }

     public abstract bool ArgumentIsRequired { get; }
}