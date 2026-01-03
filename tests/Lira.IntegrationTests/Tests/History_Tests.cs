using System.Text.Json;
using Lira.Contracts;

namespace Lira.IntegrationTests.Tests;

public class History_Tests : TestBase
{
    [Test]
    public async Task HappyPath()
    {
        // arrange
        string rulesPath = CreateRulesPath();

        string ruleName = "withHistory";

        await File.WriteAllTextAsync(
            Path.Combine(rulesPath, "history.rules"),
            $$"""
            -------------------- rule

            GET /withHistory

            ----- options

            name    = {{ruleName}}
            history = true

            ----- response

            ~ code
            200

            ~ headers
            header1: 1
            header1: 2
            header2: 9

            ~ body
            it's body
            """
        );

        await using var factory = new TestApplicationFactory(rulesPath, new AppMocks());
        var httpClient = factory.CreateDefaultClient();

        // act
        await httpClient.SendAsync(CreateRequestMessage());
        var response = await httpClient.SendAsync(CreateRequestMessage());

        // assert

        // validate rule response

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(response.Headers.GetValues("header1").ToArray(), Is.EquivalentTo(new[] {"1", "2"}));
        Assert.That(response.Headers.GetValues("header2").ToArray(), Is.EquivalentTo(new[] {"9"}));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Is.EqualTo("it's body"));

        // validate history response

        var responseHistory = await httpClient.GetAsync($"/sys/api/history/{ruleName}");

        Assert.That((int)responseHistory.StatusCode, Is.EqualTo(200));
        var bod = await responseHistory.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<GetHistoryResponse>(bod, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        Assert.That(dto.Invokes.Count, Is.EqualTo(2));
        AssertInvokeIsValid(dto.Invokes[0]);
        AssertInvokeIsValid(dto.Invokes[1]);

        static HttpRequestMessage CreateRequestMessage()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/withHistory?a=1&a=2&b=asd");
            req.Headers.Add("a", "123");
            req.Headers.Add("a", "456");
            req.Headers.Add("b", "asd");
            return req;
        }

        static void AssertInvokeIsValid(RuleInvoke invoke)
        {
            Assert.That(invoke.Time.Date, Is.EqualTo(DateTime.UtcNow.Date));
            Assert.That(invoke.Request.Path, Is.EqualTo("/withHistory"));
            Assert.That(invoke.Request.Query, Is.EqualTo("?a=1&a=2&b=asd"));
            Assert.That(invoke.Request.Headers["a"], Is.EqualTo("123,456"));
            Assert.That(invoke.Request.Headers["b"], Is.EqualTo("asd"));

            Assert.That(invoke.Result.Response, Is.Not.EqualTo(null));
            Assert.That(invoke.Result.Fault, Is.False);
            Assert.That(invoke.Result.Response.Body, Is.EqualTo("it's body"));
            Assert.That(invoke.Result.Response.Code, Is.EqualTo(200));
            Assert.That(invoke.Result.Response.Headers.Count, Is.EqualTo(2));
            Assert.That(invoke.Result.Response.Headers["header1"], Is.EqualTo("1,2"));
            Assert.That(invoke.Result.Response.Headers["header2"], Is.EqualTo("9"));
        }
    }

    [Test]
    public async Task RuleWithFault()
    {
        // arrange
        string rulesPath = CreateRulesPath();

        string ruleName = "withHistory";

        await File.WriteAllTextAsync(
            Path.Combine(rulesPath, "history.rules"),
            $$"""
            -------------------- rule

            GET /withHistory

            ----- options

            name    = {{ruleName}}
            history = true

            ----- response

            ~ fault
            """
        );

        await using var factory = new TestApplicationFactory(rulesPath, new AppMocks());
        var httpClient = factory.CreateDefaultClient();

        // act
        var req = new HttpRequestMessage(HttpMethod.Get, "/withHistory?a=1&a=2&b=asd");
        req.Headers.Add("a", "123");
        req.Headers.Add("a", "456");
        req.Headers.Add("b", "asd");

        Assert.ThrowsAsync<Exception>(() => httpClient.SendAsync(req));

        // validate history response

        var responseHistory = await httpClient.GetAsync($"/sys/api/history/{ruleName}");

        Assert.That((int)responseHistory.StatusCode, Is.EqualTo(200));
        var bod = await responseHistory.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<GetHistoryResponse>(bod, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        Assert.That(dto.Invokes.Count, Is.EqualTo(1));

        var invoke = dto.Invokes.First();
        Assert.That(invoke.Time.Date, Is.EqualTo(DateTime.UtcNow.Date));
        Assert.That(invoke.Request.Path, Is.EqualTo("/withHistory"));
        Assert.That(invoke.Request.Query, Is.EqualTo("?a=1&a=2&b=asd"));
        Assert.That(invoke.Request.Headers["a"], Is.EqualTo("123,456"));
        Assert.That(invoke.Request.Headers["b"], Is.EqualTo("asd"));

        Assert.That(invoke.Result.Response, Is.EqualTo(null));
        Assert.That(invoke.Result.Fault, Is.True);
    }
}
