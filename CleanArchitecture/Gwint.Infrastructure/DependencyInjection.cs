using Gwint.Application.Abstractions.Persistence;
using Gwint.Application.Abstractions.Security;
using Gwint.Infrastructure.Persistence;
using Gwint.Infrastructure.Persistence.Repositories;
using Gwint.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gwint.Infrastructure;

/// <summary>
/// Rejestracja adapterów infrastruktury. To tutaj PORTY (abstrakcje z warstwy
/// aplikacji) zostają związane z konkretnymi ADAPTERAMI - realizacja zasady
/// odwrócenia zależności w punkcie kompozycji.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Brak connection stringa 'DefaultConnection'.");

        services.AddDbContext<GwintDbContext>(options => options.UseNpgsql(connectionString));

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = new JwtOptions
        {
            SecretKey = jwtSection["SecretKey"] ?? string.Empty,
            Issuer = jwtSection["Issuer"] ?? "gwint-api",
            Audience = jwtSection["Audience"] ?? "gwint-client"
        };
        services.AddSingleton(jwtOptions);

        // Porty -> Adaptery
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IDeckRepository, DeckRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
