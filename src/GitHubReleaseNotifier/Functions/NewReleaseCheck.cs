using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GitHubReleaseNotifier.Models;
using GitHubReleaseNotifier.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Octokit;

namespace GitHubReleaseNotifier
{
    public class NewReleaseCheck
    {
#if DEBUG
        const string scheduleExpression = "0 */2 * * * *";
#else
        const string scheduleExpression = "0 * 0/2 * * *";
#endif
        [FunctionName(nameof(NewReleaseCheck))]
        public async Task Run(
            [TimerTrigger(scheduleExpression)]TimerInfo myTimer, 
            [Table(Constants.Repositories.TableName)] CloudTable table,
            ILogger log)
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var github = new GitHubClient(new ProductHeaderValue($"{typeof(NewReleaseCheck).GetType().Namespace}", version));

            TableContinuationToken token = null;
            var repositoriesToCheck = new List<RepositoryEntity>();
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<RepositoryEntity>(), token);
                repositoriesToCheck.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            foreach(var repositoryToCheck in repositoriesToCheck)
            {
                var sourceRepository = $"{repositoryToCheck.Owner}/{repositoryToCheck.Name}";

                log.LogInformation($"Checking for new release in {sourceRepository}...");
                var latestSourceRelease = await github.Repository.Release.GetLatest(repositoryToCheck.Owner, repositoryToCheck.Name);

                if (repositoryToCheck.LastUpdated == null || repositoryToCheck.LastUpdated < latestSourceRelease.PublishedAt)
                {
                    await SlackHelper.SendReleaseNotification($"New release {latestSourceRelease.Name} in {sourceRepository} was found.", log);

                    repositoryToCheck.LastUpdated = DateTime.UtcNow;
                    await table.ExecuteAsync(TableOperation.Replace(repositoryToCheck));

                    continue;
                }

                log.LogInformation($"No new release in {sourceRepository} was found. Next check will be at {myTimer.Schedule.GetNextOccurrence(DateTime.Now)}", log);
            }
        }
    }
}
