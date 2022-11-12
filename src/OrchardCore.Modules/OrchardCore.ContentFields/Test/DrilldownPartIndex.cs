using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DealerSolutions.Tests;

public class DrilldownPartIndex : MapIndex
{
    public string ContentItemId { get; set; }

    public string YearContentItemId { get; set; }

    public string MakeContentItemId { get; set; }
    public string ModelContentItemId { get; set; }
    public string SeriesContentItemId { get; set; }
    public string StyleContentItemId { get; set; }
}

public class DrilldownPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<DrilldownPartIndex>()
            .When(contentItem => contentItem.Published && contentItem.Has<DrilldownPart>())
            .Map(contentItem =>
            {
                var part = contentItem.As<DrilldownPart>();

                return new DrilldownPartIndex()
                {
                    ContentItemId = contentItem.ContentItemId,
                    YearContentItemId = part.YearContentId,
                    MakeContentItemId = part.MakeContentId,
                    ModelContentItemId = part.ModelContentId,
                    SeriesContentItemId = part.SeriesContentId,
                    StyleContentItemId = part.StyleContentId,
                };
            });
    }
}

