using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Seo.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Seo.Services;

public class RobotsMiddleware
{
    public const string RobotsFileName = "robots.txt";

    private readonly RequestDelegate _next;
    private readonly ISiteService _siteService;
    private readonly IStaticFileProvider _staticFileProvider;
    private readonly AdminOptions _adminOptions;

    public RobotsMiddleware(
        RequestDelegate next,
        ISiteService siteService,
        IStaticFileProvider staticFileProvider,
        IOptions<AdminOptions> adminOptions)
    {
        _next = next;
        _siteService = siteService;
        _staticFileProvider = staticFileProvider;
        _adminOptions = adminOptions.Value;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithSegments("/" + RobotsFileName))
        {
            var file = _staticFileProvider.GetFileInfo(RobotsFileName);

            if (file.Exists)
            {
                await _next(httpContext);

                return;
            }

            var settings = (await _siteService.GetSiteSettingsAsync()).As<RobotsSettings>();
            httpContext.Response.Clear();
            httpContext.Response.ContentType = "text/plain";

            if (!String.IsNullOrEmpty(settings.FileContent))
            {
                await httpContext.Response.WriteAsync(settings.FileContent);
            }
            else
            {
                var defaultContent = $"User-agent: *\r\nAllow: /\r\nDisallow: /{_adminOptions.AdminUrlPrefix}";

                await httpContext.Response.WriteAsync(defaultContent);
            }
        }

        await _next(httpContext);
    }
}
