using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportFlow.Infrastructure.Data;

namespace SupportFlow.Infrastructure;

public static class DependencyInjection 
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString)
        );
        return services;
    }
}

