using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportFlow.Application.Commands.Auth;
using SupportFlow.Application.Interfaces;
using SupportFlow.Infrastructure.Data;
using SupportFlow.Infrastructure.Repositories;
using SupportFlow.Application.Commands.Tickets;
using SupportFlow.Application.Commands.Users;
using SupportFlow.Application.Queries.Users;
using SupportFlow.Application.Queries.Tickets;
using SupportFlow.Infrastructure.Security;

namespace SupportFlow.Infrastructure;

public static class DependencyInjection 
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("SupportFlow")));
        services.AddScoped<ITicketRepository, TicketRepository>();
        
        services.AddScoped<CreateTicketCommandHandler>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<CreateUserCommandHandler>();
        services.AddScoped<GetUserByIdQueryHandler>();
        services.AddScoped<GetUsersQueryHandler>();
        services.AddScoped<GetTicketsQueryHandler>();
        services.AddScoped<GetTicketByIdQueryHandler>();
        services.AddScoped<ResolveTicketCommandHandler>();
        services.AddScoped<AssignTicketCommandHandler>();
        services.AddScoped<CloseTicketCommandHandler>();
        services.AddScoped<ReopenTicketCommandHandler>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<IJwtTokenGenerator, JwTokenGenerator>();
        services.AddScoped<GetMyTicketsQueryHandler>();
        services.AddScoped<GetMyAssignedTicketsQueryHandler>();
        
        return services;
    }
}

