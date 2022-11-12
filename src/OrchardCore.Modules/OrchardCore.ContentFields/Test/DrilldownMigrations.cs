using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace DealerSolutions.Tests;

public class DrilldownMigrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateMapIndexTable<DrilldownPartIndex>(table => table
            .Column<string>("ContentItemId", column => column.WithLength(26))
            .Column<string>("YearContentItemId", column => column.WithLength(26))
            .Column<string>("MakeContentItemId", column => column.WithLength(26))
            .Column<string>("ModelContentItemId", column => column.WithLength(26))
            .Column<string>("SeriesContentItemId", column => column.WithLength(26))
            .Column<string>("StyleContentItemId", column => column.WithLength(26)));

        SchemaBuilder.AlterIndexTable<DrilldownPartIndex>(table => table
            .CreateIndex("IDX_DrilldownPartIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "YearContentItemId",
                "MakeContentItemId",
                "ModelContentItemId",
                "SeriesContentItemId",
                "StyleContentItemId")
            );

        return 1;
    }
}
