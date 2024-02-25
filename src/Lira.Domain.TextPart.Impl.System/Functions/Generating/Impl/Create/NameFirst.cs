using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.TextPart.Impl.System.Functions.Generating.Impl.Create;

internal class NameFirst : FunctionBase, IObjectTextPart
{
    public override string Name => "name.first";

    public dynamic? Get(RuleExecutingContext context) => Next();

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
