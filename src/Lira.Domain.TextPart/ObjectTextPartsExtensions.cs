using System.Dynamic;
using System.Text;
using System.Text.Json;
using Lira.Domain.Handling.Generating;

namespace Lira.Domain.TextPart;

public static class ObjectTextPartsExtensions
{
    public static dynamic? Generate(this IReadOnlyCollection<IObjectTextPart> parts,
        RuleExecutingContext context)
    {
        int counter = 0;
        StringBuilder? sb = null;
        dynamic? firstObj = null;

        foreach (var part in parts)
        {
            foreach (var obj in part.Get(context))
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
        }

        return sb?.ToString() ?? firstObj;
    }

    public static dynamic? Generate(this IObjectTextPart part, RuleExecutingContext context)
    {
        int counter = 0;
        StringBuilder? sb = null;
        dynamic? firstObj = null;

        foreach (var obj in part.Get(context))
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

        return sb?.ToString() ?? firstObj;
    }

    public static IEnumerable<dynamic?> GetAllObjects(this IEnumerable<IObjectTextPart> parts,
        RuleExecutingContext context)
    {
        foreach (var part in parts)
        {
            foreach (var obj in part.Get(context))
            {
                yield return obj;
            }
        }
    }

    public static TextPartsProvider WrapToTextParts(this IReadOnlyCollection<IObjectTextPart> parts)
    {
        return new TextPartsProvider(parts.Select(x => new TextPartsAdapter(x)).ToArray());
    }

    private record TextPartsAdapter(IObjectTextPart ObjectTextPart) : ITextParts
    {
        public IEnumerable<string> Get(RuleExecutingContext context)
        {
            foreach (var objPart in ObjectTextPart.Get(context))
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
}