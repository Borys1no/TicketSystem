using Microsoft.EntityFrameworkCore;
using SupportFlow.Domain.Entities;
namespace  SupportFlow.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly
            );
        
    }
}

