using System.Collections.Generic;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace DealerSolutions.Tests;

public class DrilldownPartIndexAliasProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = "drilldownPart",
                Index = nameof(DrilldownPartIndex),
                IndexType = typeof(DrilldownPartIndex)
            }
        };

    public IEnumerable<IndexAlias> GetAliases()
    {
        return _aliases;
    }
}
