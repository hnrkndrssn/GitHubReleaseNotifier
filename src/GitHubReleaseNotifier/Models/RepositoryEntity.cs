using System;
using System.Collections.Generic;
using GitHubReleaseNotifier.Converters;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace GitHubReleaseNotifier.Models
{
    public class RepositoryEntity : TableEntity
    {
        public RepositoryEntity(string owner, string name)
        {
            PartitionKey = owner;
            RowKey = name;
            Subscribers = new List<string>();
        }

        public RepositoryEntity()
        {
        }

        [IgnoreProperty]
        public string Owner => PartitionKey;

        [IgnoreProperty]
        public string Name => RowKey;

        public DateTime? LastUpdated { get; set; }

        [EntityPropertyConverter(typeof(List<string>))]
        public IList<string> Subscribers { get; set; }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            EntityPropertyConvert.Serialize(this, results);

            return results;
            
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            EntityPropertyConvert.DeSerialize(this, properties);
        }
    }
}
