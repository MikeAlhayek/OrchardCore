using OrchardCore.Contents.Indexing;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.Contents;

public class ProfileMigrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateMapIndexTable<ContainedProfilePartIndex>(table => table
            .Column<string>("ProfileContentItemId", c => c.NotNull().WithLength(26))
            .Column<int>("ContentItemId", c => c.NotNull().WithLength(26))
        );

        SchemaBuilder.AlterIndexTable<ContainedProfilePartIndex>(table => table
            .CreateIndex("IDX_ContainedProfilePartIndex_DocumentId", "DocumentId", "ProfileContentItemId", "ContentItemId")
        );

        return 1;
    }
}
