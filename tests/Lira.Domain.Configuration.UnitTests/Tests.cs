using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;

namespace Lira.Domain.Configuration.UnitTests;

public class Tests
{
    // read variable
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


    // read function
    [TestCase(
        "int a = $read.from.variable;",
        "[:c int a = ][:r $read.from.variable][:c ;]")]
    [TestCase(
        "int a =   $read.from.variable;",
        "[:c int a =   ][:r $read.from.variable][:c ;]")]
    [TestCase(
        "int a   =   $read.from.variable;",
        "[:c int a   =   ][:r $read.from.variable][:c ;]")]
    [TestCase(
        "int a   =\n   $read.from.variable;",
        "[:c int a   =\n   ][:r $read.from.variable][:c ;]")]
    [TestCase(
        "if($read.from.variable == a)",
        "[:c if(][:r $read.from.variable][:c == a)]")]
    [TestCase(
        "jpath: $.amount",
        "[:c jpath: $.amount]")]

    // write

    [TestCase(
        "$$read.from.variable = a;",
        "[:w $$read.from.variable][:c = a;]")]
    [TestCase(
        "$$read.from.variable   = a;",
        "[:w $$read.from.variable][:c = a;]")]
    [TestCase(
        "$$read.from.variable =   a;",
        "[:w $$read.from.variable][:c =   a;]")]
    [TestCase(
        "$$read.from.variable =\n   a;",
        "[:w $$read.from.variable][:c =\n   a;]")]
    public void ReadVariable(string code, string expected)
    {
        var tokens = CodeParser.Parse(code);

        var result = string.Join("", tokens);
        Assert.That(result, Is.EqualTo(expected));
    }
}