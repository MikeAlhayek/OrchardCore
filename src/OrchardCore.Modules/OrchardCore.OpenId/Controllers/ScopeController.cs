using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Controllers
{
    [Admin, Feature(OpenIdConstants.Features.Management)]
    public class ScopeController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;
        private readonly IOpenIdScopeManager _scopeManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly ShellSettings _shellSettings;
        private readonly IShellHost _shellHost;
        private readonly dynamic New;

        public ScopeController(
            IOpenIdScopeManager scopeManager,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<ScopeController> stringLocalizer,
            IAuthorizationService authorizationService,
            IHtmlLocalizer<ScopeController> htmlLocalizer,
            INotifier notifier,
            ShellDescriptor shellDescriptor,
            ShellSettings shellSettings,
            IShellHost shellHost)
        {
            _scopeManager = scopeManager;
            New = shapeFactory;
            _siteService = siteService;
            S = stringLocalizer;
            _authorizationService = authorizationService;
            _notifier = notifier;
            _shellDescriptor = shellDescriptor;
            _shellSettings = shellSettings;
            _shellHost = shellHost;
        }

        public async Task<ActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
            {
                return Forbid();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var count = await _scopeManager.CountAsync();

            var model = new OpenIdScopeIndexViewModel
            {
                Pager = (await New.Pager(pager)).TotalItemCount(count)
            };

            await foreach (var scope in _scopeManager.ListAsync(pager.PageSize, pager.GetStartIndex()))
            {
                model.Scopes.Add(new OpenIdScopeEntry
                {
                    Description = await _scopeManager.GetDescriptionAsync(scope),
                    DisplayName = await _scopeManager.GetDisplayNameAsync(scope),
                    Id = await _scopeManager.GetPhysicalIdAsync(scope),
                    Name = await _scopeManager.GetNameAsync(scope)
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
            {
                return Forbid();
            }

            var model = new CreateOpenIdScopeViewModel();

            foreach (var tenant in _shellHost.GetSettingsByState(TenantState.Running))
            {
                model.Tenants.Add(new CreateOpenIdScopeViewModel.TenantEntry
                {
                    Current = String.Equals(tenant.Name, _shellSettings.Name),
                    Name = tenant.Name
                });
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOpenIdScopeViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
            {
                return Forbid();
            }

            if (await _scopeManager.FindByNameAsync(model.Name) != null)
            {
                ModelState.AddModelError(nameof(model.Name), S["The name is already taken by another scope."]);
            }

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var descriptor = new OpenIdScopeDescriptor
            {
                Description = model.Description,
                DisplayName = model.DisplayName,
                Name = model.Name
            };

            if (!String.IsNullOrEmpty(model.Resources))
            {
                descriptor.Resources.UnionWith(model.Resources.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            descriptor.Resources.UnionWith(model.Tenants
                .Where(tenant => tenant.Selected)
                .Where(tenant => !String.Equals(tenant.Name, _shellSettings.Name))
                .Select(tenant => OpenIdConstants.Prefixes.Tenant + tenant.Name));

            await _scopeManager.CreateAsync(descriptor);

            if (String.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index");
            }

            return this.LocalRedirect(returnUrl, true);
        }

        public async Task<IActionResult> Edit(string id, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
            {
                return Forbid();
            }

            var scope = await _scopeManager.FindByPhysicalIdAsync(id);
            if (scope == null)
            {
                return NotFound();
            }

            var model = new EditOpenIdScopeViewModel
            {
                Description = await _scopeManager.GetDescriptionAsync(scope),
                DisplayName = await _scopeManager.GetDisplayNameAsync(scope),
                Id = await _scopeManager.GetPhysicalIdAsync(scope),
                Name = await _scopeManager.GetNameAsync(scope)
            };

            var resources = await _scopeManager.GetResourcesAsync(scope);

            model.Resources = String.Join(" ",
                from resource in resources
                where !String.IsNullOrEmpty(resource) && !resource.StartsWith(OpenIdConstants.Prefixes.Tenant, StringComparison.Ordinal)
                select resource);

            foreach (var tenant in _shellHost.GetSettingsByState(TenantState.Running))
            {
                model.Tenants.Add(new EditOpenIdScopeViewModel.TenantEntry
                {
                    Current = String.Equals(tenant.Name, _shellSettings.Name),
                    Name = tenant.Name,
                    Selected = resources.Contains(OpenIdConstants.Prefixes.Tenant + tenant.Name)
                });
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditOpenIdScopeViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
            {
                return Forbid();
            }

            var scope = await _scopeManager.FindByPhysicalIdAsync(model.Id);
            if (scope == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var other = await _scopeManager.FindByNameAsync(model.Name);
                if (other != null && !String.Equals(
                    await _scopeManager.GetIdAsync(other),
                    await _scopeManager.GetIdAsync(scope)))
                {
                    ModelState.AddModelError(nameof(model.Name), S["The name is already taken by another scope."]);
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var descriptor = new OpenIdScopeDescriptor();
            await _scopeManager.PopulateAsync(descriptor, scope);

            descriptor.Description = model.Description;
            descriptor.DisplayName = model.DisplayName;
            descriptor.Name = model.Name;

            descriptor.Resources.Clear();

            if (!String.IsNullOrEmpty(model.Resources))
            {
                descriptor.Resources.UnionWith(model.Resources.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            descriptor.Resources.UnionWith(model.Tenants
                .Where(tenant => tenant.Selected)
                .Where(tenant => !String.Equals(tenant.Name, _shellSettings.Name))
                .Select(tenant => OpenIdConstants.Prefixes.Tenant + tenant.Name));

            await _scopeManager.UpdateAsync(scope, descriptor);

            if (String.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index");
            }

            return this.LocalRedirect(returnUrl, true);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
            {
                return Forbid();
            }

            var scope = await _scopeManager.FindByPhysicalIdAsync(id);
            if (scope == null)
            {
                return NotFound();
            }

            await _scopeManager.DeleteAsync(scope);

            return RedirectToAction(nameof(Index));
        }
    }
}
