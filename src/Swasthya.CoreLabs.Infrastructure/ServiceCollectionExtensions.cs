using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.Infrastructure.Persistence.Results;

namespace Swasthya.CoreLabs.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwasthyaPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("CoreLabs");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services;
        }

        services.AddDbContext<CoreLabDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services.AddScoped<IPrincipalRepository, PrincipalRepository>();
        services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<ILaboratoryCatalogRepository, LaboratoryCatalogRepository>();
        services.AddScoped<ITestCatalogRepository, TestCatalogRepository>();
        services.AddScoped<IPanelRepository, PanelRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAccessionRepository, AccessionRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IResultRepository, ResultRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
