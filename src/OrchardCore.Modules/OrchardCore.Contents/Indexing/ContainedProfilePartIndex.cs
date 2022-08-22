using System;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Models;
using YesSql.Indexes;

namespace OrchardCore.Contents.Indexing;
public class ContainedProfilePartIndex : MapIndex
{
    public string ProfileContentItemId { get; set; }

    public string ContentItemId { get; set; }
}

public class ContainedProfilePartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<ContainedProfilePartIndex>()
               .Map(contentItem =>
               {
                   var profilePart = contentItem.As<ContainedProfilePart>();

                   if (String.IsNullOrWhiteSpace(profilePart?.ProfileContentItemId))
                   {
                       return null;
                   }

                   return new ContainedProfilePartIndex()
                   {
                       ProfileContentItemId = profilePart.ProfileContentItemId,
                       ContentItemId = contentItem.ContentItemId,
                   };
               });
    }
}
