using System.Diagnostics;
using Lira.Common;
using Moq;
using Moq.Contrib.HttpClient;
using Lira.Common.Extensions;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;
using Lira.ExternalCalling.Http.Caller;
using Lira.FileSectionFormat;
using Lira.FileSectionFormat.Extensions;
using Microsoft.Extensions.Logging;

namespace Lira.IntegrationTests.Tests;

public class Rules_Tests : TestBase
{
    private static readonly string RulesFolderName = "rules";
    //private static readonly string RulesFolderName = "debug";

    public static string[] Cases
    {
        get
        {
            string testsDirectory = GetFixturesDirectory();
            string[] testsFiles = Directory.GetFiles(testsDirectory, "*.test", SearchOption.AllDirectories);

            // for pretty view in test explorer
            var prettyFileNames = testsFiles.Select(f =>
            {
                int index = f.IndexOf(RulesFolderName, StringComparison.Ordinal);
                var substr = f.Substring(index).TrimStart(RulesFolderName);
                return substr;
            }).ToArray();

            return prettyFileNames;
        }
    }

    [TestCaseSource(nameof(Cases))]
    public async Task RuleIsWork(string prettyTestFileName)
    {
        string fixturesDirectory = GetFixturesDirectory();
        var mocks = new AppMocks
        {
            PeImagesCache = PeImagesCache
        };

        await using var factory = new TestApplicationFactory(fixturesDirectory, mocks);

        var httpClient = factory.CreateDefaultClient();

        Console.WriteLine("Execute file: " + prettyTestFileName);
        string realTestFilePath = fixturesDirectory + prettyTestFileName;

        try
        {
            var sectionsRoot = await SectionFileParser.Parse(realTestFilePath);

            foreach (var caseSection in sectionsRoot.Sections)
            {
                var delay = caseSection.GetBlockValueOrDefault<TimeSpan>("delay");

                if (delay != TimeSpan.Zero)
                    await Task.Delay(delay);

                var sw = Stopwatch.StartNew();

                var req = CreateRequest(caseSection);

                var expectedSection = caseSection.GetSingleChildSection("expected");
                HttpResponseMessage res;
                try
                {
                    res = await httpClient.SendAsync(req);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("The application aborted the request") && expectedSection.ExistBlock("aborted"))
                        continue;
                    throw;
                }

                var wait = caseSection.GetBlockValueOrDefault<TimeSpan>("wait");

                if (wait != TimeSpan.Zero)
                    await Task.Delay(wait);

                var elapsed = expectedSection.GetBlockValueOrDefault<TimeSpan>("elapsed");
                if (elapsed != TimeSpan.Zero)
                    Assert.That(sw.Elapsed, Is.GreaterThan(elapsed));

                int expectedHttpCode = expectedSection.GetBlockValue<int>("code");
                Assert.That((int)res.StatusCode, Is.EqualTo(expectedHttpCode));

                var bodyBlock = expectedSection.GetBlockOrNull("body");

                if (bodyBlock != null)
                {
                    string expectedBody = expectedSection.GetStringValueFromRequiredBlock("body").Replace("<empty>", "");

                    string body = await res.Content.ReadAsStringAsync();
                    Assert.That(body, Is.EqualTo(expectedBody));
                }

                AssertValidHeaders(res, expectedSection);

                var httpCallSection = expectedSection.ChildSections.FirstOrDefault(x => x.Name == "action.call.http");
                AsserCallHttp(httpCallSection, mocks);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Test {realTestFilePath}:line 1 fault", e);
        }
    }

    private static void AsserCallHttp(FileSection? httpCallSection, AppMocks mocks)
    {
        if (httpCallSection != null)
        {
            string methodAndPath = httpCallSection.GetSingleLine();
            (string method, string path) = methodAndPath.SplitToTwoPartsRequired(" ").Trim();

            mocks.HttpMessageHandler.VerifyRequest(async message =>
            {
                var expectedHeadersBlock = httpCallSection.GetBlockOrNull("headers");
                if (expectedHeadersBlock != null)
                {
                    var expectedHeaders = expectedHeadersBlock.Lines;
                    foreach (var expectedHeader in expectedHeaders)
                    {
                        (string headerName, string expectedValue) = expectedHeader.SplitToTwoPartsRequired(":").Trim();
                        string actualValue = message.Headers.FirstOrDefault(x => x.Key == headerName).Value.First();

                        Assert.That(expectedValue, Is.EqualTo(actualValue));
                    }
                }

                Assert.That(method, Is.EqualTo(message.Method.ToString()));
                Assert.That(path, Is.EqualTo(message.RequestUri!.ToString()));

                var bodyBlock = httpCallSection.GetBlockOrNull("body");
                if (bodyBlock != null)
                {
                    string expectedBody = bodyBlock.GetLinesAsString();
                    Assert.That(expectedBody, Is.EqualTo(await message.Content!.ReadAsStringAsync()));
                }

                return true;
            }, times: Times.Once());
        }
    }

    private static string GetFixturesDirectory() => Path.Combine(GetCurrentDirectory(), "fixtures", RulesFolderName);

    private static void AssertValidHeaders(HttpResponseMessage res, FileSection expectedSection)
    {
        var headersRaw = expectedSection.GetLinesFromBlockOrEmpty("headers");
        foreach (var keyValueHeaderValue in headersRaw)
        {
            string[] splitted = keyValueHeaderValue.Split(':');
            string headerName = splitted[0];
            string expectedHeaderValue = splitted[1].Trim();

            var allHeaders = res.GetAllHeaders();
            if (!allHeaders.TryGetValue(headerName, out var headers))
                throw new Exception($"Not found header '{headerName}' in response");

            var headerActualValue = headers.Single();
            Assert.That(headerActualValue, Is.EqualTo(expectedHeaderValue));
        }
    }

    private static HttpRequestMessage CreateRequest(FileSection caseSection)
    {
        string methodAndPath = caseSection.LinesWithoutBlock.Single();
        string[] splitted = methodAndPath.Split(' ');

        var httpMethod = new HttpMethod(splitted[0]);

        var req = new HttpRequestMessage();
        req.Method = httpMethod;
        req.RequestUri = new Uri(splitted[1], UriKind.Relative);

        var headersLines = caseSection.GetLinesFromBlockOrEmpty("headers");
        foreach (var headerLine in headersLines)
        {
            splitted = headerLine.Split(':');
            string name = splitted[0];
            string value = splitted[1].Trim();

            req.Headers.Add(name, value);
        }

        req.Content = new StringContent(caseSection.GetStringValueFromBlockOrEmpty("body"));

        return req;
    }

    readonly static PeImagesCache PeImagesCache = new(new ConsoleLoggerFactory());

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        PeImagesCache.TryGet(new Hash([1]), out _);
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
        PeImagesCache.Dispose();
    }
}

class ConsoleLoggerFactory : ILoggerFactory
{
    private static readonly Logger _logger = new();

    public void Dispose()
    {

    }

    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotImplementedException();
    }

    public ILogger CreateLogger(string categoryName) => _logger;

    class Logger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Console.WriteLine(formatter(state, exception));
        }
    }
}