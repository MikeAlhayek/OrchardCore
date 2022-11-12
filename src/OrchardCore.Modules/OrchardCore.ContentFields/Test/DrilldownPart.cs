using OrchardCore.ContentManagement;

namespace DealerSolutions.Tests;

public class DrilldownPart : ContentPart //, IMapFrom<DrilldownPart>
{
    public string YearContentId { get; set; }

    public string MakeContentId { get; set; }

    public string ModelContentId { get; set; }

    public string SeriesContentId { get; set; }

    public string StyleContentId { get; set; }
}
