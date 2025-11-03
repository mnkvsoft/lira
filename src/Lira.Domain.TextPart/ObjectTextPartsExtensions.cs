using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using Lira.Domain.Handling.Generating;

namespace Lira.Domain.TextPart;

public static class ObjectTextPartsExtensions
{
    // public static dynamic? Generate(this IReadOnlyCollection<IObjectTextPart> parts,
    //     RuleExecutingContext context)
    // {
    //     int counter = 0;
    //     StringBuilder? sb = null;
    //     dynamic? firstObj = null;
    //
    //     foreach (var part in parts)
    //     {
    //         foreach (var obj in part.Get(context))
    //         {
    //             if (counter == 0)
    //             {
    //                 firstObj = obj;
    //             }
    //             else if (counter == 1)
    //             {
    //                 sb = new StringBuilder();
    //                 sb.Append(GetStringValue(firstObj));
    //                 sb.Append(GetStringValue(obj));
    //             }
    //             else
    //             {
    //                 sb!.Append(GetStringValue(obj));
    //             }
    //
    //             counter++;
    //         }
    //     }
    //
    //     return sb?.ToString() ?? firstObj;
    // }

    public static string? GenerateString(this IObjectTextPart part, RuleExecutingContext context) => GetStringValue(part.Generate(context));
    public static string? GenerateString(this IEnumerable<dynamic> enumerableDynamic) => GetStringValue(Generate(enumerableDynamic));

    public static dynamic? Generate(this IObjectTextPart part, RuleExecutingContext context)
    {
        if (!part.TryGetEnumerableDynamic(context, out var enumerableDynamic))
            return part.Get(context);

        return Generate(enumerableDynamic);
    }

    private static dynamic Generate(this IEnumerable<dynamic> enumerableDynamic)
    {
        int counter = 0;
        StringBuilder? sb = null;
        dynamic? firstObj = null;

        foreach (var obj in enumerableDynamic)
        {
            if (counter == 0)
            {
                firstObj = obj;
            }
            else if (counter == 1)
            {
                sb = new StringBuilder();
                sb.Append(GetStringValue(firstObj));
                sb.Append(GetStringValue(obj));
            }
            else
            {
                sb!.Append(GetStringValue(obj));
            }

            counter++;
        }

        return sb?.ToString() ?? firstObj ?? throw new Exception("result is null");
    }

    public static IEnumerable<dynamic?> GetAllObjects(this IEnumerable<IObjectTextPart> parts,
        RuleExecutingContext context)
    {
        foreach (var part in parts)
        {
            foreach (var obj in part.GetEnumerable(context))
            {
                yield return obj;
            }
        }
    }

    public static TextPartsProvider WrapToTextParts(this IEnumerable<IObjectTextPart> parts)
    {
        return new TextPartsProvider(parts.Select(x => new TextPartsAdapter(x)));
    }

    public static TextPartsProvider WrapToTextParts(this IObjectTextPart part)
    {
        return new TextPartsProvider(ToEnumerable());

        IEnumerable<ITextParts> ToEnumerable()
        {
            yield return new TextPartsAdapter(part);
        }
    }

    private record TextPartsAdapter(IObjectTextPart ObjectTextPart) : ITextParts
    {
        public IEnumerable<string> Get(RuleExecutingContext context)
        {
            var arr = ObjectTextPart.GetEnumerable(context).ToArray();
            foreach (var objPart in arr)
            {
                yield return GetStringValue(objPart);
            }
        }
    }

    public static string? GetStringValue(dynamic? obj)
    {
        if (obj == null)
            return null;

        if (obj is DateTime date)
            return date.ToString("O");

        if (obj is ExpandoObject)
            return JsonSerializer.Serialize(obj);

        return obj.ToString();
    }

    public static IEnumerable<dynamic?> GetEnumerable(this IEnumerable<IObjectTextPart> parts, RuleExecutingContext context)
    {
        foreach (var part in parts)
        {
            foreach (var value in part.GetEnumerable(context))
            {
                yield return value;
            }
        }
    }

    public static IEnumerable<dynamic?> GetEnumerable(this IObjectTextPart part, RuleExecutingContext context)
    {
        if (part.TryGetEnumerableDynamic(context, out var enumerable))
        {
            var arr = enumerable.ToArray();
            foreach (dynamic value in arr)
            {
                yield return value;
            }
        }
        else
        {
            yield return part.Get(context);
        }
    }

    private static bool TryGetEnumerableDynamic(this IObjectTextPart part, RuleExecutingContext context, [MaybeNullWhen(false)]out IEnumerable<dynamic> enumerable)
    {
        enumerable = null;
        if (part.Type != DotNetType.EnumerableDynamic)
            return false;

        enumerable = part.Get(context) ??
               throw new Exception(
                   $"{nameof(IObjectTextPart)} of type {DotNetType.EnumerableDynamic} cannot return null");
        return true;
    }



    // public static IEnumerable<dynamic> GetEnumerable(this IObjectTextPart part, RuleExecutingContext context)
    // {
    //     var value = part.Get(context);
    //
    //     foreach (var part in parts)
    //     {
    //         if (part.Type == DotNetType.EnumerableDynamic)
    //         {
    //             IEnumerable<dynamic> enumerable = (IEnumerable<dynamic>)part.Get(context);
    //             foreach (dynamic value in enumerable)
    //             {
    //                 yield return value;
    //             }
    //         }
    //         else
    //         {
    //             yield return part.Get(context);
    //         }
    //     }
    // }
}