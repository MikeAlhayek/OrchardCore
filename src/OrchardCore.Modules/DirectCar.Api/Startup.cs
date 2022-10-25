using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace DirectCar.Api;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApi", Version = "v1" });
        });
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        builder.UseSwagger();
        //var swaggerApiDefinition = serviceProvider.GetService<IOpenApiDefinition>();
        var shellSettings = serviceProvider.GetService<ShellSettings>();
        var tenantUrlPrefix = String.IsNullOrEmpty(shellSettings.RequestUrlPrefix) ? shellSettings.RequestUrlPrefix : shellSettings.RequestUrlPrefix + "/";

        //options.SwaggerEndpoint($"/{tenantUrlPrefix}swagger/{swaggerApiDefinition.Version}/swagger.json", $"{swaggerApiDefinition.Name} {swaggerApiDefinition.Version}");
    }
}
