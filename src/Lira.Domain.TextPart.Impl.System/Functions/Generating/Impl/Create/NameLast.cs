using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class NameLast : FunctionBase, IObjectTextPart
{
    public override string Name => "name.last";

    public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(Next());
    public ReturnType ReturnType => ReturnType.String;

    public static string Next() => LastNames.Random();

    private readonly static string[] LastNames =
    {
        "Black",
        "Diggory",
        "Dumbledore",
        "Granger",
        "Hagrid",
        "Lestrange",
        "Longbottom",
        "Lovegood",
        "Lupin",
        "Malfoy",
        "McGonagall",
        "Potter",
        "Scamander",
        "Snape",
        "Umbridge",
        "Voldemort",
        "Weasley"
    };
}
