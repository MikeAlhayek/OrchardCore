using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Sms.Services;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Drivers;

public class SmsSettingsDisplayDriver : SectionDisplayDriver<ISite, SmsSettings>
{
    public const string GroupId = "sms";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly SmsProviderOptions _smsProviderOptions;

    public SmsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<SmsProviderOptions> smsProviderOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _smsProviderOptions = smsProviderOptions.Value;
    }

    public override async Task<IDisplayResult> EditAsync(SmsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        return Initialize<SmsSettingsViewModel>("SmsSettings_Edit", model =>
        {
            model.DefaultProvider = settings.DefaultProviderName;
            model.Providers = _smsProviderOptions.Providers.Keys
            .Select(provider => new SelectListItem(provider, provider))
            .ToArray();
        }).Location("Content:3").OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(SmsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        var model = new SmsSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.DefaultProviderName = model.DefaultProvider;

        return await EditAsync(settings, context);
    }
}
