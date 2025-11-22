using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Econ;

namespace WeaponSkins;

public static class EconParserServiceInjection
{
    public static IServiceCollection AddEconParserService(this IServiceCollection services)
    {
        return services.AddSingleton<EconParserService>();
    }

    public static IServiceProvider UseEconParserService(this IServiceProvider provider)
    {
        provider.GetRequiredService<EconParserService>();
        return provider;
    }
}