using GraphQL.Types;

namespace DealerSolutions.Tests;

public class DrilldownPartObjectType : ObjectGraphType<DrilldownPart>
{
    public DrilldownPartObjectType()
    {
        Name = nameof(DrilldownPart);

        // Map the fields you want to expose
        Field(x => x.YearContentId, nullable: true);
        Field(x => x.MakeContentId, nullable: true);
        Field(x => x.ModelContentId, nullable: true);
        Field(x => x.SeriesContentId, nullable: true);
        Field(x => x.StyleContentId, nullable: true);
    }
}

public class DrilldownPartInputObjectType : InputObjectGraphType<DrilldownPart>
{
    public DrilldownPartInputObjectType()
    {
        Name = $"{nameof(DrilldownPart)}Input";

        Field(x => x.YearContentId, nullable: true).Description("The Id of the Year this vehicle belongs to.");
        Field(x => x.MakeContentId, nullable: true).Description("The Id of the Make this vehicle belongs to.");
        Field(x => x.ModelContentId, nullable: true).Description("The Id of the Model this vehicle belongs to.");
        Field(x => x.SeriesContentId, nullable: true).Description("The Id of the Series this vehicle belongs to.");
        Field(x => x.StyleContentId, nullable: true).Description("The Id of the Style this vehicle belongs to.");
    }
}
