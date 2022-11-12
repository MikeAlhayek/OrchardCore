using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace DealerSolutions.Tests;

public class VehicleMetadataMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public VehicleMetadataMigrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public int Create()
    {
        _contentDefinitionManager.AlterTypeDefinition("Vehicle", type => type
                .Creatable()
                .Listable()
              .WithPart("DrilldownPart", part => part
                  .WithDisplayName("Drilldown info")
                  .WithPosition("1")
              )
              .WithPart("TitlePart", part => part
                    .WithPosition("2")
                )

          );

        return 1;
    }
}
