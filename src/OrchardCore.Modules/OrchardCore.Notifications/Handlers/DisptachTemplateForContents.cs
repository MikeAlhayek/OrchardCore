using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Notifications.Core;

namespace OrchardCore.Notifications.Handlers;

public class DisptachTemplateForContents : IContentHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationMessageProvider _notificationMessageProvider;
    private readonly IEnumerable<INotificationSender> _notificationSenders;
    private readonly ILogger _logger;
    private IContentManager _contentManager;

    public DisptachTemplateForContents(IServiceProvider serviceProvider,
        INotificationMessageProvider notificationMessageProvider,
        IEnumerable<INotificationSender> notificationSenders,
        ILogger<DisptachTemplateForContents> logger)
    {
        _serviceProvider = serviceProvider;
        _notificationMessageProvider = notificationMessageProvider;
        _notificationSenders = notificationSenders;
        _logger = logger;
    }

    public async Task ActivatingAsync(ActivatingContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Activating");
    }

    public async Task ActivatedAsync(ActivatedContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Activated");
    }

    public async Task InitializingAsync(InitializingContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Initializing");
    }

    public async Task InitializedAsync(InitializingContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Initialized");
    }

    public async Task CreatingAsync(CreateContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Creating");
    }

    public async Task CreatedAsync(CreateContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Created");
    }

    public async Task LoadingAsync(LoadContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Loading");
    }

    public async Task LoadedAsync(LoadContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Loaded");
    }

    public async Task ImportingAsync(ImportContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Importing");
    }

    public async Task ImportedAsync(ImportContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Imported");
    }

    public async Task UpdatingAsync(UpdateContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Updating");
    }

    public async Task UpdatedAsync(UpdateContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Updated");
    }

    public async Task ValidatingAsync(ValidateContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Validating");
    }

    public async Task ValidatedAsync(ValidateContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Validated");
    }

    public async Task RestoringAsync(RestoreContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Restoring");
    }

    public async Task RestoredAsync(RestoreContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Restored");
    }

    public async Task VersioningAsync(VersionContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Versioning");
    }

    public async Task VersionedAsync(VersionContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Versioned");
    }

    public async Task DraftSavingAsync(SaveDraftContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "DraftSaving");
    }

    public async Task DraftSavedAsync(SaveDraftContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "DraftSaved");
    }

    public async Task PublishingAsync(PublishContentContext context)
    {
        await HandleEventAsync(context.PublishingItem, "Publishing");
    }

    public async Task PublishedAsync(PublishContentContext context)
    {
        await HandleEventAsync(context.PublishingItem, "Published");
    }

    public async Task UnpublishingAsync(PublishContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Unpublishing");
    }

    public async Task UnpublishedAsync(PublishContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Unpublished");
    }

    public async Task RemovingAsync(RemoveContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Removing");
    }

    public async Task RemovedAsync(RemoveContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Removed");
    }

    public Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        return Task.CompletedTask;
    }

    public async Task CloningAsync(CloneContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Cloning");
    }

    public async Task ClonedAsync(CloneContentContext context)
    {
        await HandleEventAsync(context.ContentItem, "Cloned");
    }

    private async Task HandleEventAsync(ContentItem contentItem, string eventName)
    {
        var templatePickerPart = contentItem.As<NotificationTemplatePickerPart>();

        if (String.Equals(templatePickerPart?.EventName, eventName, StringComparison.OrdinalIgnoreCase))
        {
            var templateIds = templatePickerPart.NotificationTemplateContentItemIds ?? Array.Empty<string>();

            await DispatchTemplatesAsync(templateIds, contentItem);
        }
    }

    private async Task DispatchTemplatesAsync(string[] templateIds, ContentItem contentItem)
    {
        if (templateIds == null || templateIds.Length == 0)
        {
            return;
        }

        _contentManager ??= _serviceProvider.GetRequiredService<IContentManager>();

        var templates = await _contentManager.GetAsync(templateIds);

        var messages = await _notificationMessageProvider.GetAsync(templates, new Dictionary<string, string>()
        {
            { "DisplayText", contentItem.DisplayText },
            { "ContentItemId", contentItem.ContentItemId },
            { "ContentType", contentItem.ContentType },
            { "ModifiedUtc", contentItem.ModifiedUtc?.ToString() },
            { "PublishedUtc", contentItem.PublishedUtc?.ToString() },
            { "CreatedUtc", contentItem.CreatedUtc?.ToString() },
            { "Owner", contentItem.Owner },
            { "Author", contentItem.Author },
        });

        foreach (var message in messages)
        {
            foreach (var user in message.Users)
            {
                await _notificationSenders.InvokeAsync(async sender => await sender.TrySendAsync(user, message), _logger);
            }
        }
    }
}
