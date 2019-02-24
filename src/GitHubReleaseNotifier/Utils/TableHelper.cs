using System;
using System.Threading.Tasks;
using GitHubReleaseNotifier.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace GitHubReleaseNotifier.Utils
{
    public static class TableHelper
    {
        public static async Task<(bool success, string error)> UpdateRepositoryEntity(RepositoryEntity existing, RepositoryResource resource, CloudTable table, ILogger log)
        {
            if (!existing.Subscribers.Contains(resource.Subscriber))
            {
                log.LogInformation($"");

                try
                {
                    existing.Subscribers.Add(resource.Subscriber);
                    var updateOperation = TableOperation.Replace(existing);
                    await table.ExecuteAsync(updateOperation);
                }
                catch (StorageException sex)
                {
                    return (false, sex.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
            return (true, "");
        }

        public static async Task<(bool success, string error)> InsertRepositoryEntity(RepositoryResource resource, CloudTable table, ILogger log)
        {
            log.LogInformation($"");

            try
            {
                var newEntity = new RepositoryEntity(resource.Owner, resource.Name);
                newEntity.Subscribers.Add(resource.Subscriber);
                var insertOperation = TableOperation.Insert(newEntity);
                await table.ExecuteAsync(insertOperation);
            }
            catch (StorageException sex)
            {
                return (false, sex.RequestInformation.ExtendedErrorInformation.ErrorMessage);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (true, "");
        }
    }
}
