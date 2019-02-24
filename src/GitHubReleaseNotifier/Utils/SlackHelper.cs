using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GitHubReleaseNotifier.Utils
{
    public static class SlackHelper
    {
        private static readonly string slackWebHookUrl = Environment.GetEnvironmentVariable("SlackWebHookUrl");

        public static async Task SendReleaseNotification(string message, ILogger log)
        {
            if (string.IsNullOrEmpty(slackWebHookUrl)) return;

            log.LogInformation(message);

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsJsonAsync(slackWebHookUrl, new { text = message });
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred when sending the message to Slack");
            }
        }

    }
}
