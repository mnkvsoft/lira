using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class NameFirst : FunctionBase, IObjectTextPart
{
    public override string Name => "name.first";

    public Task<dynamic?> Get(RuleExecutingContext context) => Task.FromResult<dynamic?>(Next());
    public ReturnType ReturnType => ReturnType.String;

    public static string Next() => FirstNames.Random();

    private readonly static string[] FirstNames =
  {
        "Sirius",
        "Cedric",
        "Albus",
        "Hermione",
        "Rubeus",
        "Bellatrix",
        "Neville",
        "Luna",
        "Remus",
        "Draco",
        "Minerva",
        "Harry",
        "Newt",
        "Severus",
        "Dolores",
        "Lord",
        "Fred",
        "George",
        "Ginny",
        "Ron"
    };
}
