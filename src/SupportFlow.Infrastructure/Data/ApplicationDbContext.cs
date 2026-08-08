using Microsoft.EntityFrameworkCore;
using SupportFlow.Domain.Entities;
using SupportFlow.Domain.Enums;

namespace SupportFlow.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        
    }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(t => t.Description)
                .HasMaxLength(1000);
            entity.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(t => t.Priority)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(t => t.CreateAt)
                .IsRequired();

            entity.HasOne(t => t.CreatedByUserId)
                .WithMany()
                .HasForeignKey("CreatedByUserId")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.AssignTo)
                .WithMany()
                .HasForeignKey("AssignedToId")
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);
            entity.HasIndex(u => u.Email)
                .IsUnique();
            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(u => u.Department)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(100);
            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(30);

        });
    }
}