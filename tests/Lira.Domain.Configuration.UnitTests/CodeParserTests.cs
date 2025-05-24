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
    public void ReadRuleVariable(string code, string expected)
    {
        var declaredItems = new DeclaredItems {
            new RuntimeRuleVariable("$$read.from.variable", valueType: null),
        };

        var (codeBlock, _, _) = CodeParser.Parse(
            code,
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(
        "int a = $read.from.variable;",
        "[:c int a = ][:r $read.from.variable][:c ;]")]
    [TestCase(
        "var a = $read.from.variable.ToString();",
        "[:c var a = ][:r $read.from.variable][:c .ToString();]")]
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
        "$\"\"\"some string\"\"\"",
        "[:c $\"\"\"some string\"\"\"]")]
    public void ReadLocalVariable(string code, string expected)
    {
        var declaredItems = new DeclaredItems {
            new LocalVariable("$read.from.variable", valueType: null)
        };

        var (codeBlock, _, _) = CodeParser.Parse(
            code,
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ReadRuleVariableWithPropertyAccess()
    {
        var declaredItems = new DeclaredItems { new DeclaredRuleVariable("$$person", [Mock.Of<IObjectTextPart>()], valueType: null) };

        var (codeBlock, _, _) = CodeParser.Parse(
            "$$person.name",
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo("[:r $$person][:c .name]"));
    }

    [Test]
    public void ReadLocalVariableWithPropertyAccess()
    {
        var declaredItems = new DeclaredItems { new LocalVariable("$person", valueType: null) };

        var (codeBlock, _, _) = CodeParser.Parse(
            "$person.name",
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo("[:r $person][:c .name]"));
    }

    [TestCase(
        "int a = #read.from.function;",
        "[:c int a = ][:r #read.from.function][:c ;]")]
    [TestCase(
        "int a =   #read.from.function;",
        "[:c int a =   ][:r #read.from.function][:c ;]")]
    [TestCase(
        "int a   =   #read.from.function;",
        "[:c int a   =   ][:r #read.from.function][:c ;]")]
    [TestCase(
        "int a   =\n   #read.from.function;",
        "[:c int a   =\n   ][:r #read.from.function][:c ;]")]
    [TestCase(
        "if(#read.from.function == a)",
        "[:c if(][:r #read.from.function][:c == a)]")]
    [TestCase(
        "jpath: $.amount",
        "[:c jpath: $.amount]")]

    public void ReadFunction(string code, string expected)
    {
        var declaredItems = new DeclaredItems
        {
            new Function("#read.from.function",
                Array.Empty<IObjectTextPart>(), valueType: null)
        };

        var (codeBlock, _, _) = CodeParser.Parse(
            code,
            declaredItems);

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(
        "$$write.to.variable = a;",
        "[:w $$write.to.variable][:c = a;]")]
    [TestCase(
        "$$write.to.variable   = a;",
        "[:w $$write.to.variable][:c = a;]")]
    [TestCase(
        "$$write.to.variable =   a;",
        "[:w $$write.to.variable][:c =   a;]")]
    [TestCase(
        "$$write.to.variable =\n   a;",
        "[:w $$write.to.variable][:c =\n   a;]")]
    // local
    [TestCase(
        "$write.to.variable = a;",
        "[:w $write.to.variable][:c = a;]")]
    [TestCase(
        "$write.to.variable   = a;",
        "[:w $write.to.variable][:c = a;]")]
    [TestCase(
        "$write.to.variable =   a;",
        "[:w $write.to.variable][:c =   a;]")]
    [TestCase(
        "$write.to.variable =\n   a;",
        "[:w $write.to.variable][:c =\n   a;]")]
    public void WriteVariable(string code, string expected)
    {
        var (codeBlock, _, _) = CodeParser.Parse(
            code,
            new DeclaredItems());

        var result = string.Concat(codeBlock.Tokens);
        Assert.That(result, Is.EqualTo(expected));
    }
}