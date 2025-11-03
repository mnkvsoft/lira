using Lira.Domain.Configuration.Rules.Parsers.CodeParsing;
using Lira.Domain.Configuration.Rules.ValuePatternParsing;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;
using Moq;

namespace Lira.Domain.Configuration.UnitTests;

public class CodeParserTests
{
    private static readonly CodeParser Sut = new(Array.Empty<IKeyWordInDynamicBlock>());

    [TestCase(
        "int a = $$read_from_variable;",
        "[:c int a = ][:r $$read_from_variable][:c ;]")]
    [TestCase(
        "var a = $$read_from_variable.ToString();",
        "[:c var a = ][:r $$read_from_variable][:c .ToString();]")]
    [TestCase(
        "int a =   $$read_from_variable;",
        "[:c int a =   ][:r $$read_from_variable][:c ;]")]
    [TestCase(
        "int a   =   $$read_from_variable;",
        "[:c int a   =   ][:r $$read_from_variable][:c ;]")]
    [TestCase(
        "int a   =\n   $$read_from_variable;",
        "[:c int a   =\n   ][:r $$read_from_variable][:c ;]")]
    [TestCase(
        "if($$read_from_variable == a)",
        "[:c if(][:r $$read_from_variable][:c == a)]")]
    [TestCase(
        "$$\"\"\"some string\"\"\"",
        "[:c $$\"\"\"some string\"\"\"]")]
    public void ReadRuleVariable(string code, string expected)
    {
        var declaredItems = new DeclaredItems {
            new RuntimeRuleVariable("$$read_from_variable", CreateTypeInfo()),
        };

        var (codeBlock, _, _) = Sut.Parse(
            code,
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

    private static TypeInfo CreateTypeInfo()
    {
        return new TypeInfo(typeof(string), castTo: null);
    }

    [TestCase(
        "int a = $read_from_variable;",
        "[:c int a = ][:r $read_from_variable][:c ;]")]
    [TestCase(
        "var a = $read_from_variable.ToString();",
        "[:c var a = ][:r $read_from_variable][:c .ToString();]")]
    [TestCase(
        "int a =   $read_from_variable;",
        "[:c int a =   ][:r $read_from_variable][:c ;]")]
    [TestCase(
        "int a   =   $read_from_variable;",
        "[:c int a   =   ][:r $read_from_variable][:c ;]")]
    [TestCase(
        "int a   =\n   $read_from_variable;",
        "[:c int a   =\n   ][:r $read_from_variable][:c ;]")]
    [TestCase(
        "if($read_from_variable == a)",
        "[:c if(][:r $read_from_variable][:c == a)]")]
    [TestCase(
        "$\"\"\"some string\"\"\"",
        "[:c $\"\"\"some string\"\"\"]")]
    public void ReadLocalVariable(string code, string expected)
    {
        var declaredItems = new DeclaredItems {
            new LocalVariable("$read_from_variable", CreateTypeInfo())
        };

        var (codeBlock, _, _) = Sut.Parse(
            code,
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ReadRuleVariableWithPropertyAccess()
    {
        var declaredItems = new DeclaredItems { new DeclaredRuleVariable("$$person", Mock.Of<IObjectTextPart>(), CreateTypeInfo()) };

        var (codeBlock, _, _) = Sut.Parse(
            "$$person.name",
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo("[:r $$person][:c .name]"));
    }

    [Test]
    public void ReadLocalVariableWithPropertyAccess()
    {
        var declaredItems = new DeclaredItems { new LocalVariable("$person", CreateTypeInfo()) };

        var (codeBlock, _, _) = Sut.Parse(
            "$person.name",
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo("[:r $person][:c .name]"));
    }

    [TestCase(
        "int a = @read.from.function;",
        "[:c int a = ][:r @read.from.function][:c ;]")]
    [TestCase(
        "int a =   @read.from.function;",
        "[:c int a =   ][:r @read.from.function][:c ;]")]
    [TestCase(
        "int a   =   @read.from.function;",
        "[:c int a   =   ][:r @read.from.function][:c ;]")]
    [TestCase(
        "int a   =\n   @read.from.function;",
        "[:c int a   =\n   ][:r @read.from.function][:c ;]")]
    [TestCase(
        "if(@read.from.function == a)",
        "[:c if(][:r @read.from.function][:c == a)]")]
    [TestCase(
        "jpath: $.amount",
        "[:c jpath: $.amount]")]

    public void ReadFunction(string code, string expected)
    {
        var declaredItems = new DeclaredItems
        {
            new Function(
                "@read.from.function",
                Mock.Of<IObjectTextPart>(),
                CreateTypeInfo())
        };

        var (codeBlock, _, _) = Sut.Parse(
            code,
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(
        "$$write_to_variable = a;",
        "[:w $$write_to_variable][:c = a;]")]
    [TestCase(
        "$$write_to_variable   = a;",
        "[:w $$write_to_variable][:c = a;]")]
    [TestCase(
        "$$write_to_variable =   a;",
        "[:w $$write_to_variable][:c =   a;]")]
    [TestCase(
        "$$write_to_variable =\n   a;",
        "[:w $$write_to_variable][:c =\n   a;]")]
    // local
    [TestCase(
        "$write_to_variable = a;",
        "[:w $write_to_variable][:c = a;]")]
    [TestCase(
        "$write_to_variable   = a;",
        "[:w $write_to_variable][:c = a;]")]
    [TestCase(
        "$write_to_variable =   a;",
        "[:w $write_to_variable][:c =   a;]")]
    [TestCase(
        "$write_to_variable =\n   a;",
        "[:w $write_to_variable][:c =\n   a;]")]
    public void WriteVariable(string code, string expected)
    {
        var (codeBlock, _, _) = Sut.Parse(
            code,
            new DeclaredItems());

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }
}