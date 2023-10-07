//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Lira.Domain.Models.RulesModel.Matching;
//using Lira.Domain.Models.RulesModel.Matching.Matchers.Body.SpecificFormatMatchers;

//namespace Lira.UnitTests;

//public class FormBodyPattern_Tests
//{
//    [Test]
//    public async Task IsMatch_ValidPath_PassValidArgumnetToSingleValuePattern()
//    {
//        Mock<RequestData> request = new Mock<RequestData>();
//        request
//            .Setup(x => x.Body)
//            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("key1=value1&key2=value2")));

//        var pattern = new FormBodyRequestMatcher(Mock.Of<ILoggerFactory>(), new Dictionary<string, ValuePattern>
//        {
//            {"key1", new ValuePattern.Static("value1")}
//        });

//        var result = await pattern.IsMatch(request.Object);

//        Assert.True(result);
//    }
//}
