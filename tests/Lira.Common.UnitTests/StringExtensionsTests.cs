using Lira.Common.Extensions;

namespace Lira.Common.UnitTests;

public class Tests
{

    [Test]
    public void JsonAligned()
    {
        var result = StringExtensions.AlignIndents(
            [
                "        {",
                "             'age': 5",
                "        }"
            ],
            count: 4
        );

        Assert.That(
            result, Is.EqualTo(
                new[]
                {
                    "    {",
                    "         'age': 5",
                    "    }"
                }));
    }

    [Test]
    public void JsonBracketShifted()
    {
        var result = StringExtensions.AlignIndents(
            [
                "      {",
                "             'age': 5",
                "        }"
            ],
            count: 4
        );

        Assert.That(
            result, Is.EqualTo(
                new[]
                {
                    "    {",
                    "           'age': 5",
                    "      }"
                }));
    }
}