using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Domain.Entities;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Identity;

namespace IkerFinance.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public new IQueryable<ApplicationUser> Users => Set<ApplicationUser>();
    public IQueryable<Currency> Currencies => Set<Currency>();
    public IQueryable<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public IQueryable<Category> Categories => Set<Category>();
    public IQueryable<Transaction> Transactions => Set<Transaction>();
    public IQueryable<Budget> Budgets => Set<Budget>();

    public new void Add<T>(T entity) where T : class => Set<T>().Add(entity);
    public new void Remove<T>(T entity) where T : class => Set<T>().Remove(entity);
    public new void Update<T>(T entity) where T : class => Set<T>().Update(entity);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
        });

        builder.Entity<Currency>(entity =>
        {
            entity.ToTable("Currencies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Symbol).HasMaxLength(5).IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();
        });

        builder.Entity<ExchangeRate>(entity =>
        {
            entity.ToTable("ExchangeRates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rate).HasPrecision(18, 6).IsRequired();

            entity.HasOne(e => e.FromCurrency)
                .WithMany(c => c.FromExchangeRates)
                .HasForeignKey(e => e.FromCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToCurrency)
                .WithMany(c => c.ToExchangeRates)
                .HasForeignKey(e => e.ToCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasIndex(e => new { e.FromCurrencyId, e.ToCurrencyId, e.EffectiveDate });
        });

        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.Icon).HasMaxLength(50);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasIndex(e => new { e.UserId, e.Name });
        });

        builder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.ConvertedAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasOne(e => e.Currency)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConvertedCurrency)
                .WithMany()
                .HasForeignKey(e => e.ConvertedCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.UserId, e.Date });
        });

        builder.Entity<Budget>(entity =>
        {
            entity.ToTable("Budgets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.AlertAt80Percent).HasPrecision(5, 2);
            entity.Property(e => e.AlertAt100Percent).HasPrecision(5, 2);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasOne(e => e.Currency)
                .WithMany(c => c.Budgets)
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.StartDate, e.EndDate });
            entity.HasIndex(e => new { e.UserId, e.CategoryId, e.Period, e.IsActive })
                .HasFilter("\"IsActive\" = true");
        });
    }
}