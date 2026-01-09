using System.Text.Json.Nodes;

namespace Lira.Common.UnitTests;

public class ObjectPath_Tests
{
    [Test]
    public void StringField()
    {
        var path = ObjectPath.Parse("name");
        var json = JsonNode.Parse(
            """
            {
                "name": "Mason Power"
            }
            """)!;

       var result = path.TryGetStringValue(json, out var value);

       Assert.IsTrue(result);
       Assert.That(value, Is.EqualTo("Mason Power"));
    }

    [Test]
    public void DecimalField()
    {
        var path = ObjectPath.Parse("balance");
        var json = JsonNode.Parse(
            """
            {
                "balance": 12.45
            }
            """)!;

        var result = path.TryGetStringValue(json, out var value);

        Assert.IsTrue(result);
        Assert.That(value, Is.EqualTo("12.45"));
    }

    [Test]
    public void BoolField()
    {
        var path = ObjectPath.Parse("is_free");
        var json = JsonNode.Parse(
            """
            {
                "is_free": true
            }
            """)!;

        var result = path.TryGetStringValue(json, out var value);

        Assert.IsTrue(result);
        Assert.That(value, Is.EqualTo("true"));
    }

    [Test]
    public void ObjectField()
    {
        var path = ObjectPath.Parse("person");
        var json = JsonNode.Parse(
            """
            {
              "person":
              {
                "name": "Mason Power"
              }
            }
            """)!;

        var result = path.TryGetStringValue(json, out var value);

        Assert.IsTrue(result);
        Assert.That(value, Is.EqualTo(
            """
            {
              "name": "Mason Power"
            }
            """));
    }

    [Test]
    public void NestedFields()
    {
        var path = ObjectPath.Parse("person.name");
        var json = JsonNode.Parse(
            """
            {
              "person":
              {
                "name": "Mason Power"
              }
            }
            """)!;

        var result = path.TryGetStringValue(json, out var value);

        Assert.IsTrue(result);
        Assert.That(value, Is.EqualTo("Mason Power"));
    }


    [Test]
    public void ArrayField()
    {
        var path = ObjectPath.Parse("persons[1].name");
        var json = JsonNode.Parse(
            """
            {
              "persons": [
                  {
                    "name": "Mason Power"
                  },
                  {
                    "name": "Baranius Zimmer"
                  }
              ]
            }
            """)!;

        var result = path.TryGetStringValue(json, out var value);

        Assert.IsTrue(result);
        Assert.That(value, Is.EqualTo("Baranius Zimmer"));
    }


    [TestCase("missingField")]
    [TestCase("persons[2]")]
    [TestCase("persons[0].missingField")]
    [TestCase("persons[0].name.missingField")]
    public void NegativeCases(string strPath)
    {
        var path = ObjectPath.Parse(strPath);
        var json = JsonNode.Parse(
            """
            {
              "persons": [
                  {
                    "name": "Mason Power"
                  },
                  {
                    "name": "Baranius Zimmer"
                  }
              ]
            }
            """)!;

        var result = path.TryGetStringValue(json, out var value);

        Assert.IsFalse(result);
        Assert.Null(value);
    }
}