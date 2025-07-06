using Lira.Domain.Configuration.Parsing;

namespace Lira.Domain.Configuration.UnitTests;

[TestFixture]
public class TokenParserTests
{
    private TokenParser _parser;
    private List<OperatorDefinition> _operatorDefinitions;

    [SetUp]
    public void Setup()
    {
        _operatorDefinitions =
        [
            new OperatorDefinition("repeat", ParametersMode.Maybe),
            new OperatorDefinition("random", ParametersMode.None,
                allowedChildElements: new Dictionary<string, ParametersMode> { { "item", ParametersMode.Maybe } }),
            new OperatorDefinition("if", ParametersMode.Required,
                allowedChildElements: new Dictionary<string, ParametersMode>
                {
                    { "else", ParametersMode.None }
                })
        ];

        _parser = new TokenParser(_operatorDefinitions);
    }

    [Test]
    public void StaticText()
    {
        var result = _parser.Parse("Hello World");
        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<t>Hello World</t>"
        ));
    }

    [Test]
    public void SimpleOperator_With_ParentText_Without_Parameters()
    {
        var result = _parser.Parse("@repeat Content@end");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars=''>" +
            "<t>Content</t>" +
            "</op>"
        ));
    }

    [Test]
    public void SimpleOperator_With_ParentText_Without_Parameters_With_NewLine()
    {
        var result = _parser.Parse(

            "[\n" +
            "    @repeat\n" +
            "        @random\n" +
            "            @item\n" +
            "            {\n" +
            "                \"type\": \"car\"\n" +
            "            }\n" +
            "            @item\n" +
            "            {\n" +
            "                \"type\": \"bike\"\n" +
            "            }\n" +
            "        @end\n" +
            "    @end\n" +
            "]");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<t>[\n    </t>" +
            "<op name='repeat' pars=''>" +
                "<op name='random' pars=''>" +
                    "<i name='item' pars=''>" +
                        "<t>" +
                        "    {\n" +
                        "        \"type\": \"car\"\n" +
                        "    }" +
                        "</t>" +
                    "</i>" +
                    "<i name='item' pars=''>" +
                        "<t>" +
                        "    {\n" +
                        "        \"type\": \"bike\"\n" +
                        "    }" +
                        "</t>" +
                    "</i>" +
                "</op>" +
            "</op>" +
            "<t>\n]</t>"
        ));
    }

    [Test]
    public void SimpleOperator_With_ParentText_With_OneParameters()
    {
        var result = _parser.Parse("@repeat(3)Content@end");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars='(3)'>" +
            "<t>Content</t>" +
            "</op>"
        ));
    }

    [Test]
    public void SimpleOperator_With_ParentText_With_ManyParameters()
    {
        var result = _parser.Parse("@repeat(3, 4, 5)Content@end");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars='(3, 4, 5)'>" +
            "<t>Content</t>" +
            "</op>"
        ));
    }

    [Test]
    public void SimpleOperator_With_ParentText_With_NewLineInParameters()
    {
        var result = _parser.Parse("@repeat(3\n, 4\n, 5\n)Content@end");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars='(3\n, 4\n, 5\n)'>" +
            "<t>Content</t>" +
            "</op>"
        ));
    }

    [Test]
    public void SimpleOperator_With_ParentText_With_SimpleParameter()
    {
        var result = _parser.Parse("@repeat: 3 \n Content@end");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars=': 3 \n'>" +
            "<t>Content</t>" +
            "</op>"
        ));
    }

    [Test]
    public void NestedOperators()
    {
        var input = "@if($$variable == 123)@repeat(3)Text@end@end";
        var result = _parser.Parse(input);

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='if' pars='($$variable == 123)'>" +
            "<op name='repeat' pars='(3)'>" +
            "<t>Text</t>" +
            "</op>" +
            "</op>"
        ));
    }

    [Test]
    public void RandomOperator_WithItems()
    {
        var input = """
                    @random
                       @item First
                       @item(percent: 2) Second
                    @end
                    """;
        var result = _parser.Parse(input);

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='random' pars=''>" +
            "<i name='item' pars=''><t>First</t></i>" +
            "<i name='item' pars='(percent: 2)'><t>Second</t></i>" +
            "</op>"
        ));
    }

    [Test]
    public void IfOperator_WithElseConditions()
    {
        var input = """
                    @if(cond1)
                      Content
                    @else
                      Default
                    @end
                    """;
        var result = _parser.Parse(input);

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='if' pars='(cond1)'>" +
            "<t>Content</t>" +
            "<i name='else' pars=''><t>Default</t></i>" +
            "</op>"
        ));
    }

    //
    // [Test]
    // public void Parse_MissingEndTag_ThrowsException()
    // {
    //     var ex = Assert.Throws<TokenParsingException>(() =>
    //         _parser.Parse("@repeat(3)Content"));
    //
    //     Assert.That(ex.Message, Does.Contain("missing closing @end tag"));
    // }

    //
    // [Test]
    // public void Parse_MissingEndTag_ThrowsException()
    // {
    //     var ex = Assert.Throws<TokenParsingException>(() =>
    //         _parser.Parse("@repeat(3)Content"));
    //
    //     Assert.That(ex.Message, Does.Contain("missing closing @end tag"));
    // }
    //
    // [Test]
    // public void Parse_UnclosedParameters_ThrowsException()
    // {
    //     var ex = Assert.Throws<TokenParsingException>(() =>
    //         _parser.Parse("@repeat(3 Content@end"));
    //
    //     Assert.That(ex.Message, Does.Contain("Unclosed parameters"));
    // }
    //
    // [Test]
    // public void Parse_OperatorWithoutParentheses_WorksCorrectly()
    // {
    //     var result = _parser.Parse("@repeat\nContent\n@end");
    //
    //     Assert.That(result[0].Name, Is.EqualTo("repeat"));
    //     Assert.That(result[0].Parameters, Is.Null);
    //     Assert.That(result[0].Content, Is.EqualTo("\nContent\n"));
    // }
    //
    // [Test]
    // public void Parse_ItemWithoutParentheses_WorksCorrectly()
    // {
    //     var result = _parser.Parse("@random\n@item Content\n@end");
    //
    //     Assert.That(result[0].Children[0].Name, Is.EqualTo("item"));
    //     Assert.That(result[0].Children[0].Parameters, Is.Null);
    // }
    //
    // [Test]
    // public void Parse_ComplexStructure_WithMixedOperators()
    // {
    //     var input = @"Start
    //         @if(user)
    //             @repeat(3)
    //                 @random
    //                     @item A
    //                     @item B
    //                 @end
    //             @end
    //         @else
    //             Nothing
    //         @end
    //         Finish";
    //
    //     Assert.DoesNotThrow(() => _parser.Parse(input));
    // }
    //
    // [Test]
    // public void Parse_EmptyOperator_WorksCorrectly()
    // {
    //     var result = _parser.Parse("@repeat()@end");
    //
    //     Assert.That(result[0].Name, Is.EqualTo("repeat"));
    //     Assert.That(result[0].Parameters, Is.Empty);
    //     Assert.That(result[0].Content, Is.Empty);
    // }
    //
    // [Test]
    // public void Parse_SpecialCharacters_InParameters()
    // {
    //     var result = _parser.Parse("@if(a==1 && b==2)Content@end");
    //
    //     Assert.That(result[0].Parameters, Is.EqualTo("a==1 && b==2"));
    // }

    //
    // [Test]
    // public void Parse_IfOperator_WithElseConditions()
    // {
    //     var input = @"@if(cond1)
    //         Content
    //     @else if(cond2)
    //         Other
    //     @else
    //         Default
    //     @end";
    //
    //     var result = _parser.Parse(input);
    //
    //     Assert.That(result[0].Name, Is.EqualTo("if"));
    //     Assert.That(result[0].Children.Count, Is.EqualTo(2));
    //     Assert.That(result[0].Children[0].Name, Is.EqualTo("else if"));
    //     Assert.That(result[0].Children[1].Name, Is.EqualTo("else"));
    // }
}