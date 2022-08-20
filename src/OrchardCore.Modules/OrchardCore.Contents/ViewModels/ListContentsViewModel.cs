using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Contents.ViewModels
{
    public class ListContentsViewModel
    {
        public ContentOptionsViewModel Options { get; set; }

        [BindNever]
        public dynamic Header { get; set; }

        [BindNever]
        public List<dynamic> ContentItems { get; set; }

        [BindNever]
        public dynamic Pager { get; set; }
    }
}
