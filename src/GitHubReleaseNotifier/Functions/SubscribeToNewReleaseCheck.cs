using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GitHubReleaseNotifier.Models;
using GitHubReleaseNotifier.Utils;
using Microsoft.WindowsAzure.Storage.Table;

namespace GitHubReleaseNotifier.Functions
{
    public class SubscribeToNewReleaseCheck
    {
        [FunctionName(nameof(SubscribeToNewReleaseCheck))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table(Constants.Repositories.TableName)] CloudTable table,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var resource = JsonConvert.DeserializeObject<RepositoryResource>(requestBody);
            var existing = await table.ExecuteAsync(TableOperation.Retrieve<RepositoryEntity>(resource.Owner, resource.Name));
            if(existing.Result != null)
            {
                var (success, error) = await TableHelper.UpdateRepositoryEntity((RepositoryEntity)existing.Result, resource, table, log);
                if (!success)
                    return new BadRequestObjectResult(error);
            }
            else
            {
                var (success, error) = await TableHelper.InsertRepositoryEntity(resource, table, log);
                if (!success)
                    return new BadRequestObjectResult(error);
            }

            return new OkObjectResult($"Succesfully subscribed {resource.Subscriber} to GitHub repository {resource.Owner}/{resource.Name}");
        }
    }
}
