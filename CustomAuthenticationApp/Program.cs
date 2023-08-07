using CustomAuthenticationApp.Abstractions;
using CustomAuthenticationApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CustomAuthenticationApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddCustomServices();

            if (builder.HostEnvironment.IsDevelopment())
                builder.Logging.AddDebug();
            else
                builder.Logging.SetMinimumLevel(LogLevel.None);

            await builder.Build().RunAsync();
        }
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddAuthorizationCore();
        services.AddScoped<AppAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<AppAuthenticationStateProvider>());
        services.AddScoped<IAuthenticationService, AppAuthenticationService>();
        services.AddMemoryCache();
        services.AddScoped<IStorageAccessor, StorageAccessor>();
        services.AddScoped<IStorageHandler, BrowserStorageHandler>();
        return services;
    }
}

//public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
//{
//    services.AddLocalization();
//    services.Configure<RequestLocalizationOptions>(options =>
//    {
//        string defaultCulture = "en-001";
//        var cultures = new[] { defaultCulture, "en-GB", "en-US", "en" };
//        var supportedCultures = cultures.Select(c => new CultureInfo(c)).ToArray();
//        options.DefaultRequestCulture = new RequestCulture(defaultCulture);
//        options.SupportedCultures = supportedCultures;
//        options.SupportedUICultures = supportedCultures;
//    });
//    return services;
//}