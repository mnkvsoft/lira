using System;
using System.Net;
using System.Threading.Tasks;
using NBomber.CSharp;

namespace MyLoadTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using HttpClient c = new HttpClient();
            //{
            //    DefaultRequestVersion = HttpVersion.Version20,
            //    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            //};

            c.BaseAddress = new Uri("http://localhost:7057");

            var scenario = Scenario.Create("hello_world_scenario", async context =>
            {
                //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "order");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/orders/1");
                request.Content = new StringContent("{\"amount\": 124}");
                request.Headers.Add("Accept", "application/json");
                //request.Headers.Add("example", "body.jpath");
                request.Headers.Add("example", "repeat_block");

                var response = await c.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail();
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(rate: 30000,
                                  interval: TimeSpan.FromSeconds(1),
                                  during: TimeSpan.FromHours(24))
            );

            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }
    }
}