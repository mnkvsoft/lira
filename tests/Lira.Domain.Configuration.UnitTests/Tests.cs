using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.Impl;

namespace Lira.Domain.Configuration.UnitTests;

public class Tests
{
    [TestCase(
        "int a = $$read.from.variable;",
        "[:c int a = ][:r $$read.from.variable][:c ;]")]
    [TestCase(
        "var a = $$read.from.variable.ToString();",
        "[:c var a = ][:r $$read.from.variable][:c .ToString();]")]
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
        var declaredItems = new DeclaredItems();
        declaredItems.Variables.Add(new RuntimeVariable(new CustomItemName("read.from.variable"), type: null));

        var (codeBlock, _) = CodeParser.Parse(
            code,
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(
        "int a = $read.from.function;",
        "[:c int a = ][:r $read.from.function][:c ;]")]
    [TestCase(
        "int a =   $read.from.function;",
        "[:c int a =   ][:r $read.from.function][:c ;]")]
    [TestCase(
        "int a   =   $read.from.function;",
        "[:c int a   =   ][:r $read.from.function][:c ;]")]
    [TestCase(
        "int a   =\n   $read.from.function;",
        "[:c int a   =\n   ][:r $read.from.function][:c ;]")]
    [TestCase(
        "if($read.from.function == a)",
        "[:c if(][:r $read.from.function][:c == a)]")]
    [TestCase(
        "jpath: $.amount",
        "[:c jpath: $.amount]")]

    public void ReadFunction(string code, string expected)
    {
        var declaredItems = new DeclaredItems();
        declaredItems.Functions.Add(new Function(new CustomItemName("read.from.function"),
            Array.Empty<IObjectTextPart>(), type: null));

        var (codeBlock, _) = CodeParser.Parse(
            code,
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

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
    public void WriteVariable(string code, string expected)
    {
        var (codeBlock, _) = CodeParser.Parse(
            code,
            new DeclaredItems());

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }
}