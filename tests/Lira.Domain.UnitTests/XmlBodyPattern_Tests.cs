//using Moq;

//namespace Lira.UnitTests;

//public class XmlBodyPattern_Tests
//{
//    [Test]
//    public void IsMatch_ValidPath_PassValidArgumnetToSingleValuePattern()
//    {
//        var mock = new Mock<ISimpleValuePattern>();

//        var pattern = new XPathBodyPattern(new Dictionary<string, ISimpleValuePattern>
//    {
//        {"//employee[1]/text()", mock.Object}
//    });

//        string body = File.ReadAllText("./Fixtures/body.xml");
//        pattern.IsMatch(body);

//        mock.Verify(x => x.IsMatch("Johnny Dapp"), Times.Once);
//    }
//}