using System;
using System.Threading.Tasks;
using GraphQL;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using YesSql;

namespace DealerSolutions.Tests;

public class DrilldownPartGraphQLFilter : GraphQLFilter<ContentItem>
{
    public override Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        if (!context.HasPopulatedArgument("where"))
        {
            return Task.FromResult(query);
        }

        var whereArguments = JObject.FromObject(context.Arguments["where"].Value);

        if (whereArguments == null)
        {
            return Task.FromResult(query);
        }

        var drilldown = JObject.FromObject(whereArguments.Property("drilldown").Value);

        if (drilldown != null)
        {
            var drilldownQuery = query.With<DrilldownPartIndex>();

            if (drilldown.TryGetValue(nameof(DrilldownPart.YearContentId), out var yearContentId) && !String.IsNullOrWhiteSpace(yearContentId?.ToString()))
            {
                drilldownQuery.Where(x => x.YearContentItemId == yearContentId.ToString().Trim());
            }

            if (drilldown.TryGetValue(nameof(DrilldownPart.MakeContentId), out var makeContentId) && !String.IsNullOrWhiteSpace(makeContentId?.ToString()))
            {
                drilldownQuery.Where(x => x.MakeContentItemId == makeContentId.ToString().Trim());
            }

            if (drilldown.TryGetValue(nameof(DrilldownPart.ModelContentId), out var modelContentId) && !String.IsNullOrWhiteSpace(modelContentId?.ToString()))
            {
                drilldownQuery.Where(x => x.ModelContentItemId == modelContentId.ToString().Trim());
            }

            if (drilldown.TryGetValue(nameof(DrilldownPart.SeriesContentId), out var seriesContentId) && !String.IsNullOrWhiteSpace(seriesContentId?.ToString()))
            {
                drilldownQuery.Where(x => x.SeriesContentItemId == seriesContentId.ToString().Trim());
            }

            if (drilldown.TryGetValue(nameof(DrilldownPart.StyleContentId), out var styleContentId) && !String.IsNullOrWhiteSpace(styleContentId?.ToString()))
            {
                drilldownQuery.Where(x => x.StyleContentItemId == styleContentId.ToString().Trim());
            }

            return Task.FromResult(drilldownQuery.All());
        }

        return Task.FromResult(query);
    }
}
