using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Controllers;

[Feature("OrchardCore.Contents.Profile")]
[Admin]
public class ProfilePickerController : Controller
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IEnumerable<IContentPickerResultProvider> _resultProviders;

    public ProfilePickerController(
        IContentDefinitionManager contentDefinitionManager,
        IEnumerable<IContentPickerResultProvider> resultProviders
        )
    {
        _contentDefinitionManager = contentDefinitionManager;
        _resultProviders = resultProviders;
    }

    public async Task<IActionResult> SearchContentItems(string contentType, string query)
    {
        if (String.IsNullOrWhiteSpace(contentType))
        {
            return BadRequest($"{nameof(contentType)} is required parameter.");
        }

        var definition = _contentDefinitionManager.GetTypeDefinition(contentType);

        if (definition == null)
        {
            return new ObjectResult(new List<ContentPickerResult>());
        }

        var resultProvider = _resultProviders.FirstOrDefault(p => p.Name == "Default");

        var results = await resultProvider.Search(new ContentPickerSearchContext
        {
            Query = query,
            DisplayAllContentTypes = false,
            ContentTypes = new[] { contentType },
            PartFieldDefinition = null
        });

        return new ObjectResult(results.Select(r => new VueMultiselectItemViewModel() { Id = r.ContentItemId, DisplayText = r.DisplayText, HasPublished = r.HasPublished }));
    }
}
