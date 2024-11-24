using Lira.Domain.Configuration.Rules.Parsers;

namespace Lira.Domain.Configuration.UnitTests;

public class Tests
{
    [TestCase(
        "int a = $$read.from.variable;",
        "[:c int a = ][:r $$read.from.variable][:c ;]")]
    [TestCase(
        "int a =   $$read.from.variable;",
        "[:c int a =   ][:r $$read.from.variable][:c ;]")]
    [TestCase(
        "int a   =   $$read.from.variable;",
        "[:c int a   =   ][:r $$read.from.variable][:c ;]")]
    [TestCase(
        "int a   =\n   $$read.from.variable;",
        "[:c int a   =\n   ][:r $$read.from.variable][:c ;]")]
    [TestCase(
        "if($$read.from.variable == a)",
        "[:c if(][:r $$read.from.variable][:c == a)]")]
    [TestCase(
        "$$\"\"\"some string\"\"\"",
        "[:c $$\"\"\"some string\"\"\"]")]
    public void ReadVariable(string code, string expected)
    {
        var tokens = CodeParser.Parse(code);

        var result = string.Join("", tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(
        "$$read.from.variable = a;",
        "[:w $$read.from.variable][:c = a;]")]
    // [TestCase(
    //     "int a =   $$read.from.variable;",
    //     "[:c int a =   ][:r $$read.from.variable][:c ;]")]
    // [TestCase(
    //     "int a   =   $$read.from.variable;",
    //     "[:c int a   =   ][:r $$read.from.variable][:c ;]")]
    // [TestCase(
    //     "int a   =\n   $$read.from.variable;",
    //     "[:c int a   =\n   ][:r $$read.from.variable][:c ;]")]
    // [TestCase(
    //     "if($$read.from.variable == a)",
    //     "[:c if(][:r $$read.from.variable][:c == a)]")]
    // [TestCase(
    //     "$$\"\"\"some string\"\"\"",
    //     "[:c $$\"\"\"some string\"\"\"]")]
    public void WriteVariable(string code, string expected)
    {
        var tokens = CodeParser.Parse(code);

        var result = string.Join("", tokens);
        Assert.That(result, Is.EqualTo(expected));
    }
}