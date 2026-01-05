namespace Lira.IntegrationTests.Tests;

public class Rules_Tests : TestBase
{
    [Test]
    public async Task MultipleResponses()
    {
        // arrange
        string rulesPath = CreateRulesPath();

        await File.WriteAllTextAsync(
            Path.Combine(rulesPath, "test.rules"),
              """
              -------------------- rule

              GET /test

              ----- response
              ~ fault

              ----- response
              ~ code
              500

              ----- response
              {
                "status": "executed"
              }
              """
        );

        await using var factory = new TestApplicationFactory(rulesPath, new AppMocks());
        var httpClient = factory.CreateDefaultClient();

        List<Task<string>> tasks = new();

        // act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(GetResult(httpClient));
        }

        // assert
        var results = await Task.WhenAll(tasks);

        Assert.True(results.Contains("fault"));
        Assert.True(results.Contains("500"));
        Assert.True(results.Contains("200"));

        static async Task<string> GetResult(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("/test");
                return ((int)response.StatusCode).ToString();
            }
            catch (Exception)
            {
                return "fault";
            }
        }
    }
}