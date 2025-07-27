using Lira.Domain.Configuration.Parsing;

namespace Lira.Domain.Configuration.UnitTests;

[TestFixture]
public class OperatorParserTests
{

    [Test]
    public void StaticText()
    {
        var result = OperatorParser.Parse("Hello World");
        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<t>Hello World</t>"
        ));
    }

    [Test]
    public void SimpleOperator_Inline_WithParentText_WithoutParameters()
    {
        var result = OperatorParser.Parse("@repeat Content@end");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars=''>" +
            "<t>Content</t>" +
            "</op>"
        ));
    }

    [Test]
    public void SimpleOperator_Content_NewLine()
    {
        var result = OperatorParser.Parse("@repeat\nContent\n@end");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars=''>" +
            "<t>Content</t>" +
            "</op>"
        ));
    }

    [Test]
    public void SimpleOperators_With_ParentText_With_OneParameters()
    {
        var result = OperatorParser.Parse("@repeat(3)Content@end");

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
        var result = OperatorParser.Parse("@repeat(3, 4, 5)Content@end");

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
        var result = OperatorParser.Parse("@repeat(3\n, 4\n, 5\n)Content@end");

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
        var result = OperatorParser.Parse("@repeat: 3 \n Content@end");

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars=': 3 \n'>" +
            "<t>Content</t>" +
            "</op>"
        ));
    }

    [Test]
    public void NestedOperator_Simple_WithParentText_WithoutParameters()
    {
        var result = OperatorParser.Parse(
            """
                [
                    @repeat
                        @random
                            @item
                            {
                                "type": "car"
                            }
                            @item
                            {
                                "type": "bike"
                            }
                        @end
                    @end
                ]
                """.Replace("\r\n", "\n"));

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<t>[\n    </t>" +
            "<op name='repeat' pars=''>" +
            "<op name='random' pars=''>" +
            "<i name='item' pars=''>" +
            "<t>" +
            "{\n" +
            "    \"type\": \"car\"\n" +
            "}" +
            "</t>" +
            "</i>" +
            "<i name='item' pars=''>" +
            "<t>" +
            "{\n" +
            "    \"type\": \"bike\"\n" +
            "}" +
            "</t>" +
            "</i>" +
            "</op>" +
            "</op>" +
            "<t>\n]</t>"
        ));
    }

    [Test]
    public void NestedOperators_Inline()
    {
        var input = "@if($$variable == 123)@repeat(3)Text@end@end";
        var result = OperatorParser.Parse(input);

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
    public void NestedOperators_WithParameters()
    {
        var result = OperatorParser.Parse(
            """
                {
                    "orders": [
                        @repeat: 3
                        {
                            "items": [
                                @repeat(min: 1, max: 5)
                                {
                                    "id": 123
                                }
                                @end
                            ]
                        }
                        @end
                    ]
                }
                """.Replace("\r\n", "\n"));

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<t>{\n" +
            "    \"orders\": [\n" +
            "        " +
            "</t>" +
            "<op name='repeat' pars=': 3\n'>" +
            "<t>" +
            "{\n" +
            "    \"items\": [\n" +
            "        " +
            "</t>" +
            "<op name='repeat' pars='(min: 1, max: 5)'>" +
            "<t>" +
            "{\n" +
            "    \"id\": 123\n" +
            "}" +
            "</t>" +
            "</op>" +
            "<t>\n    ]\n" +
            "}" +
            "</t>" +
            "</op>" +
            "<t>\n    ]\n" +
            "}" +
            "</t>"
        ));
    }


    [Test]
    public void NestedOperators_ComplexStructure()
    {
        var result = OperatorParser.Parse(
            """
                Start
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
                Finish
                """.Replace("\r\n", "\n"));

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<t>Start\n</t>" +
            "<op name='if' pars='(user)'>" +
            "<op name='repeat' pars='(3)'>" +
            "<op name='random' pars=''>" +
            "<i name='item' pars=''><t>A</t></i>" +
            "<i name='item' pars=''><t>B</t></i>" +
            "</op>" +
            "</op>" +
            "<i name='else' pars=''><t>Nothing</t></i>" +
            "</op>" +
            "<t>\nFinish</t>"
        ));
    }

    [Test]
    public void RandomOperator_WithItemsInLine()
    {
        var input = """
                    @random
                       @item First
                       @item(percent: 2) Second
                    @end
                    """;
        var result = OperatorParser.Parse(input);

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
        var result = OperatorParser.Parse(input);

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='if' pars='(cond1)'>" +
            "<t>Content</t>" +
            "<i name='else' pars=''><t>Default</t></i>" +
            "</op>"
        ));
    }


    [Test]
    public void Parse_IfOperator_WithElseConditions()
    {
        var input =
            """
            @if(cond1)
                Content
            @else if(cond2)
                Other
            @else
                Default
            @end
            """.Replace("\r\n", "\n");

        var result = OperatorParser.Parse(input);

        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='if' pars='(cond1)'>" +
                    "<t>Content</t>" +
                "<i name='else if' pars='(cond2)'>" +
                    "<t>Other</t>" +
                "</i>" +
                "<i name='else' pars=''>" +
                    "<t>Default</t>" +
                "</i>" +
            "</op>"
        ));
    }

    [Test]
    public void MissingEndTag()
    {
        var ex = Assert.Throws<TokenParsingException>(() =>
            OperatorParser.Parse("@repeat(3)Content"));

        Assert.That(ex.Message, Is.EqualTo("Operator @repeat(3) must be closed with an @end"));
    }

    [Test]
    public void UnclosedParameters()
    {
        var ex = Assert.Throws<TokenParsingException>(() =>
            OperatorParser.Parse("@repeat(3 Content@end"));

        Assert.That(ex.Message, Is.EqualTo("Missing closing symbol ')' when defining @repeat parameters: '@repeat(3 Content@end'"));
    }

    [Test]
    public void Escaping()
    {
        var result = OperatorParser.Parse("@@repeat");
        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo("<t>@</t><t>@repeat</t>"));
    }

    [Test]
    public void EmptyOperator()
    {
        var result = OperatorParser.Parse("@repeat()@end");
        string xmlView = result.GetXmlView();
        Assert.That(xmlView, Is.EqualTo(
            "<op name='repeat' pars='()'>" +
            "</op>"
        ));
    }
}