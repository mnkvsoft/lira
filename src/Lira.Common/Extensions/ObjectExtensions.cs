namespace Lira.Common.Extensions;

public static class ObjectExtensions
{
     public static T Apply<T>(this T value, Action<T> action) where T : class
    { 
        action(value); 
        return value;
    }
}
