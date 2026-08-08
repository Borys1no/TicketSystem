using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportFlow.Application.Interfaces;
using SupportFlow.Infrastructure.Data;
using SupportFlow.Infrastructure.Repositories;

namespace SupportFlow.Infrastructure;

public class DependencyInjection
{
    public static IServiceCollection AddInfrastucture(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITicketRepository, TicketRepository>();
        
        return services;
    }
}