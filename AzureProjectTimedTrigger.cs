using System;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureProjectTimedTrigger.Function
{
    public static class AzureProjectTimedTrigger
    {
        [FunctionName("AzureProjectTimedTrigger")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var jsonString = await MakeStackOverflowRequest();

            var jsonOb = JsonConvert.DeserializeObject<dynamic>(jsonString);

            var newQuestionCount = jsonOb.items.Count;

            await MakeSlackRequest($"You have {newQuestionCount} question on Stack Overflow!");

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        public static async Task<string> MakeStackOverflowRequest()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(handler))
            {
                var response = await client.GetAsync($"https://api.stackexchange.com/2.3/search?fromdate=1635206400&order=desc&sort=activity&intitle=azure&site=stackoverflow");

                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
        public static async Task<string> MakeSlackRequest(string message)
        {
            using (var client = new HttpClient())
            {
                var requestData = new StringContent("{'text':'" + message + "'}", Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://hooks.slack.com/services/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", requestData);

                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
    }
}