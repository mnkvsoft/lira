namespace Lira.Domain.TextPart.Impl.PreDefinedFunctions.Functions;

internal interface IWithArgumentFunction : IWithArgument
{
    void SetArgument(object argument);
}

internal interface IWithArgumentFunction<in T> : IWithArgument
{
    void SetArgument(T argument);
}

internal abstract class WithArgumentFunction<T> : IWithArgumentFunction, IWithArgumentFunction<T>
{
     public abstract void SetArgument(T argument);
     public void SetArgument(object argument)
     {
         SetArgument((T)argument);
     }

     public abstract bool ArgumentIsRequired { get; }
}