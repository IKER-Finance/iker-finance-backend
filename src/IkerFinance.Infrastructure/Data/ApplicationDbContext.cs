using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Domain.Entities;

namespace IkerFinance.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Budget> Budgets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.HomeCurrency)
                .IsRequired();

            entity.Property(e => e.PreferredLanguage)
                .HasMaxLength(10);

            entity.Property(e => e.RegistrationDate)
                .IsRequired();
        });

        builder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.ConvertedAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TransactionDate);
        });

        builder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
        });
    }
}