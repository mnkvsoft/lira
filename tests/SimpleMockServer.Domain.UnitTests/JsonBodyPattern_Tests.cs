//using Moq;

//namespace SimpleMockServer.UnitTests;

//public class JsonBodyPattern_Tests
//{
//    [Test]
//    public void IsMatch_ValidPath_PassValidArgumnetToSingleValuePattern()
//    {
//        var mock = new Mock<ISimpleValuePattern>();

//        var pattern = new JsonPathBodyPattern(new Dictionary<string, ISimpleValuePattern>
//    {
//        {"$.address.city", mock.Object}
//    });

//        string body = File.ReadAllText("./Fixtures/body.json");
//        pattern.IsMatch(body);

//        mock.Verify(x => x.IsMatch("Nara"), Times.Once);
//    }
//}