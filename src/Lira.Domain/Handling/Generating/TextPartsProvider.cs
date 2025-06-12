using System.Text;

namespace Lira.Domain.Handling.Generating;

// public interface ITextPartsProvider
// {
//     IAsyncEnumerable<string> GetParts(RuleExecutingContext ctx);
// }

public class TextPartsProvider(IEnumerable<ITextParts> valueParts)
{
    public async IAsyncEnumerable<string> Get(RuleExecutingContext ctx)
    {
        foreach (var part in valueParts)
        {
            await foreach (var text in part.Get(ctx))
            {
                yield return text;
            }
        }
    }

    public async Task<string> GetSingleString(RuleExecutingContext context)
    {
        var sb = new StringBuilder();

        await foreach (var text in Get(context))
        {
            sb.Append(text);
        }

        return sb.ToString();
    }
}

// public static class TextPartsExtensions
// {
//     public static async Task<string> GetSingleString(this IEnumerable<ITextPart> parts, RuleExecutingContext context)
//     {
//         var sb = new StringBuilder();
//
//         foreach (var part in parts)
//         {
//             var texts = await part.Get(context);
//             foreach (var text in texts)
//             {
//                 if (text != null)
//                     sb.Append(text);
//             }
//         }
//
//         return sb.ToString();
//     }
// }