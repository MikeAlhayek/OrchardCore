using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmsServices(this IServiceCollection services)
    {
        services.AddScoped<DefaultSmsProvider>();
        services.AddScoped<ISmsProviderFactory, SmsProviderFactory>();

        return services;
    }

    public static IServiceCollection AddPhoneFormatValidator(this IServiceCollection services)
    {
        services.TryAddScoped<IPhoneFormatValidator, DefaultPhoneFormatValidator>();

        return services;
    }

    public static IServiceCollection AddSmsProvider<T>(this IServiceCollection services, string name)
        where T : class, ISmsProvider
    {
        services.AddScoped<T>();
        services.Configure<SmsProviderOptions>(options =>
        {
            options.Providers.Add(name, (sp) => sp.GetService<T>());
        });

        return services;
    }

    public static IServiceCollection AddTwilioProvider(this IServiceCollection services)
    {
        return services.AddSmsProvider<TwilioSmsProvider>(SmsConstants.TwilioServiceName);
    }

    public static IServiceCollection AddConsoleProvider(this IServiceCollection services)
    {
        return services.AddSmsProvider<ConsoleSmsProvider>(SmsConstants.ConsoleServiceName);
    }
}
