using System.Reflection;
using SimpleMockServer.Common.Extensions;
using SimpleMockServer.FileSectionFormat;
using SimpleMockServer.IntegrationTests.Extensions;

namespace SimpleMockServer.IntegrationTests;

public class Tests
{
    public static string[] Cases
    {
        get
        {
            string testsDirectory = GetFixturesDirectory();
            string[] testsFiles = Directory.GetFiles(testsDirectory, "*.test", SearchOption.AllDirectories);

            // for pretty view in test explorer
            var prettyFileNames = testsFiles.Select(f =>
            {
                int index = f.IndexOf("Fixtures");
                var substr = f.Substring(index).TrimStart("Fixtures");
                return substr;
            }).ToArray();

            return prettyFileNames;
        }
    }


    [TestCaseSource(nameof(Cases))]
    public async Task RuleIsWork(string prettyTestFileName)
    {
        string fixturesDirectory = GetFixturesDirectory();
        using var factory = new TestApplicationFactory(fixturesDirectory);
        var httpClient = factory.CreateHttpClient();

        Console.WriteLine("Execute file: " + prettyTestFileName);
        string realTestFilePath = fixturesDirectory + prettyTestFileName;

        var sections = await SectionFileParser.Parse(realTestFilePath, new Dictionary<string, IReadOnlySet<string>>
            {
                {"expected", new HashSet<string>{ "body", "code", "headers" } },
                {"case", new HashSet<string>{ "headers", "body" } },
            },
        maxNestingDepth: 2);

        foreach (var caseSection in sections)
        {
            var req = CreateRequest(caseSection);
            var res = await httpClient.SendAsync(req);

            var expectedSection = caseSection.GetSingleChildSection("expected");

            int expectedHttpCode = expectedSection.GetValueFromBlock<int>("code");
            Assert.AreEqual(expectedHttpCode, (int)res.StatusCode);

            var bodyBlock = expectedSection.GetBlockOrNull("body");

            if (bodyBlock != null)
            {
                string expectedBody = expectedSection.GetStringValueFromRequiredBlock("body");

                if (res.Content != null)
                {
                    string body = await res.Content.ReadAsStringAsync();
                    Assert.AreEqual(expectedBody, body);
                }
            }

            AssertValidHeaders(res, expectedSection);
        }
    }

    private static string GetFixturesDirectory()
    {
        string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string testsDirectory = Path.Combine(currentDirectory, @"Fixtures");
        return testsDirectory;
    }

    private static void AssertValidHeaders(HttpResponseMessage res, FileSection expectedSection)
    {
        var headersRaw = expectedSection.GetLinesFromBlockOrEmpty("headers");
        foreach (var keyValueHeaderValue in headersRaw)
        {
            string[] splitted = keyValueHeaderValue.Split(':');
            string headerName = splitted[0];
            string expectedHeaderValue = splitted[1].Trim();

            var allHeaders = res.GetAllHeaders();
            if (!allHeaders.ContainsKey(headerName))
                throw new Exception($"Not found header '{headerName}' in response");

            var headerActualValue = allHeaders[headerName].Single();
            Assert.AreEqual(expectedHeaderValue, headerActualValue);
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
}