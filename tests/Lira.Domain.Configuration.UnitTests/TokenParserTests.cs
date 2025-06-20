using NUnit.Framework;
using System;
using System.Collections.Generic;
using Lira.Domain.Configuration.Parsing;

[TestFixture]
public class TokenParserTests
{
    private TokenParser _parser;
    private List<OperatorDefinition> _operatorDefinitions;

    [SetUp]
    public void Setup()
    {
        _operatorDefinitions = new List<OperatorDefinition>
        {
            new OperatorDefinition { Name = "repeat" },
            new OperatorDefinition
            {
                Name = "random",
                AllowedChildElements = new List<string> { "item" }
            },
            new OperatorDefinition
            {
                Name = "if",
                AllowedChildElements = new List<string> { "else if", "else" }
            }
        };

        _parser = new TokenParser(_operatorDefinitions);
    }

    [Test]
    public void Parse_StaticText_ReturnsSingleStaticToken()
    {
        var result = _parser.Parse("Hello World");

        Assert.That(result.Count, Is.EqualTo(11)); // 11 символов
        Assert.That(result[0].Type, Is.EqualTo(TokenType.StaticData));
    }

    [Test]
    public void Parse_SimpleOperator_WithAndWithoutParentheses()
    {
        var withParens = _parser.Parse("@repeat(3)Content@end");
        var withoutParens = _parser.Parse("@repeat Content@end");

        Assert.That(withParens[0].Name, Is.EqualTo("repeat"));
        Assert.That(withParens[0].Parameters, Is.EqualTo("3"));
        Assert.That(withoutParens[0].Name, Is.EqualTo("repeat"));
        Assert.That(withoutParens[0].Parameters, Is.Null);
    }

    [Test]
    public void Parse_NestedOperators_CorrectlyBuildsHierarchy()
    {
        var input = "@if(cond1)@repeat(3)Text@end@end";
        var result = _parser.Parse(input);

        Assert.That(result[0].Name, Is.EqualTo("if"));
        Assert.That(result[0].Children[0].Name, Is.EqualTo("repeat"));
    }

    [Test]
    public void Parse_RandomOperator_WithItems()
    {
        var input = @"@random
            @item First
            @item(weight=2) Second
        @end";

        var result = _parser.Parse(input);

        Assert.That(result[0].Name, Is.EqualTo("random"));
        Assert.That(result[0].Children.Count, Is.EqualTo(2));
        Assert.That(result[0].Children[1].Parameters, Is.EqualTo("weight=2"));
    }

    [Test]
    public void Parse_IfOperator_WithElseConditions()
    {
        var input = @"@if(cond1)
            Content
        @else if(cond2)
            Other
        @else
            Default
        @end";

        var result = _parser.Parse(input);

        Assert.That(result[0].Name, Is.EqualTo("if"));
        Assert.That(result[0].Children.Count, Is.EqualTo(2));
        Assert.That(result[0].Children[0].Name, Is.EqualTo("else if"));
        Assert.That(result[0].Children[1].Name, Is.EqualTo("else"));
    }

    [Test]
    public void Parse_MissingEndTag_ThrowsException()
    {
        var ex = Assert.Throws<TokenParsingException>(() =>
            _parser.Parse("@repeat(3)Content"));

        Assert.That(ex.Message, Does.Contain("missing closing @end tag"));
    }

    [Test]
    public void Parse_UnclosedParameters_ThrowsException()
    {
        var ex = Assert.Throws<TokenParsingException>(() =>
            _parser.Parse("@repeat(3 Content@end"));

        Assert.That(ex.Message, Does.Contain("Unclosed parameters"));
    }

    [Test]
    public void Parse_OperatorWithoutParentheses_WorksCorrectly()
    {
        var result = _parser.Parse("@repeat\nContent\n@end");

        Assert.That(result[0].Name, Is.EqualTo("repeat"));
        Assert.That(result[0].Parameters, Is.Null);
        Assert.That(result[0].Content, Is.EqualTo("\nContent\n"));
    }

    [Test]
    public void Parse_ItemWithoutParentheses_WorksCorrectly()
    {
        var result = _parser.Parse("@random\n@item Content\n@end");

        Assert.That(result[0].Children[0].Name, Is.EqualTo("item"));
        Assert.That(result[0].Children[0].Parameters, Is.Null);
    }

    [Test]
    public void Parse_ComplexStructure_WithMixedOperators()
    {
        var input = @"Start
            @if(user)
                @repeat(3)
                    @random
                        @item A
                        @item B
                    @end
                @end
            @else
                Nothing
            @end
            Finish";

        Assert.DoesNotThrow(() => _parser.Parse(input));
    }

    [Test]
    public void Parse_EmptyOperator_WorksCorrectly()
    {
        var result = _parser.Parse("@repeat()@end");

        Assert.That(result[0].Name, Is.EqualTo("repeat"));
        Assert.That(result[0].Parameters, Is.Empty);
        Assert.That(result[0].Content, Is.Empty);
    }

    [Test]
    public void Parse_SpecialCharacters_InParameters()
    {
        var result = _parser.Parse("@if(a==1 && b==2)Content@end");

        Assert.That(result[0].Parameters, Is.EqualTo("a==1 && b==2"));
    }
}