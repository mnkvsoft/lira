using Lira.Common;

namespace Lira.IntegrationTests.Tests;

public class ReloadRules_Tests : TestBase
{
    private static string Nl = Constants.NewLine;
    private static readonly TimeSpan PhysicalFileProviderPoolingInterval = TimeSpan.FromSeconds(20);

    [Test]
    public async Task AddNewRule()
    {
        string rulesPath = CreateRulesPath();
        await using var factory = new TestApplicationFactory(rulesPath);
        var httpClient = factory.CreateDefaultClient();

        var response1 = await httpClient.GetAsync("/test");
        Assert.That((int)response1.StatusCode, Is.EqualTo(404));

        await File.WriteAllTextAsync(
            Path.Combine(rulesPath, "1.rules"),

            @"----- rule" + Nl +
            "GET /test" + Nl +
            "--- response" + Nl +
            "~ code" + Nl +
            "200");

        await Task.Delay(PhysicalFileProviderPoolingInterval);

        var response2 = await httpClient.GetAsync("/test");
        Assert.That((int)response2.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task DeleteRule()
    {
        string rulesPath = CreateRulesPath();

        var ruleFile = Path.Combine(rulesPath, "1.rules");
        await File.WriteAllTextAsync(
            ruleFile,

            @"----- rule" + Nl +
            "GET /test" + Nl +
            "--- response" + Nl +
            "~ code" + Nl +
            "200"

        );

        await using var factory = new TestApplicationFactory(rulesPath);
        var httpClient = factory.CreateDefaultClient();

        var response1 = await httpClient.GetAsync("/test");
        Assert.That((int)response1.StatusCode, Is.EqualTo(200));

        File.Delete(ruleFile);
        await Task.Delay(PhysicalFileProviderPoolingInterval);

        var response2 = await httpClient.GetAsync("/test");
        Assert.That((int)response2.StatusCode, Is.EqualTo(404));
    }


    [Test]
    public async Task ChangeRule()
    {
        string rulesPath = CreateRulesPath();

        var ruleFile = Path.Combine(rulesPath, "1.rules");
        await File.WriteAllTextAsync(
            ruleFile,

            @"----- rule" + Nl +
            "GET /test" + Nl +
            "--- response" + Nl +
            "~ code" + Nl +
            "200"

        );

        await using var factory = new TestApplicationFactory(rulesPath);
        var httpClient = factory.CreateDefaultClient();

        var response1 = await httpClient.GetAsync("/test");
        Assert.That((int)response1.StatusCode, Is.EqualTo(200));

        await File.WriteAllTextAsync(
            ruleFile,

            @"----- rule" + Nl +
            "GET /test" + Nl +
            "--- response" + Nl +
            "~ code" + Nl +
            "204"

        );
        await Task.Delay(PhysicalFileProviderPoolingInterval);

        var response2 = await httpClient.GetAsync("/test");
        Assert.That((int)response2.StatusCode, Is.EqualTo(204));
    }
}
