namespace Lira.IntegrationTests.Tests;

public class ExampleRules_Tests : TestBase
{
    [Test]
    public async Task ExamplesIsCompiled()
    {
        var fixturesDirectory = Path.Combine(GetCurrentDirectory(), """..\..\..\..\..\docs\examples""");
        var mocks = new AppMocks();
        await using var factory = new TestApplicationFactory(fixturesDirectory, mocks);
        var httpClient = factory.CreateDefaultClient();

        var response = await httpClient.GetAsync("/hello/world");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.EqualTo("hello world!"));
    }
}