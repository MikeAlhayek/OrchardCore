using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Elasticsearch
{
    public class Permissions : IPermissionProvider
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

        public static readonly Permission ManageElasticIndexes = ElasticsearchIndexPermissionHelper.ManageElasticIndexes;
        public static readonly Permission QueryElasticApi = new("QueryElasticsearchApi", "Query Elasticsearch Api", new[] { ManageElasticIndexes });

        public Permissions(ElasticIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync();
            var result = new List<Permission>();
            foreach (var index in elasticIndexSettings)
            {
                result.Add(ElasticsearchIndexPermissionHelper.GetElasticIndexPermission(index.IndexName));
            }

            result.Add(ManageElasticIndexes);
            result.Add(QueryElasticApi);

            return result;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageElasticIndexes }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { QueryElasticApi }
                }
            };
        }
    }
}
