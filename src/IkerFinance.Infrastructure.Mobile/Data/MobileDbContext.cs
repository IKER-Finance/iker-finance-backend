using Microsoft.EntityFrameworkCore;
using IkerFinance.Domain.Entities;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Infrastructure.Mobile.Identity;

namespace IkerFinance.Infrastructure.Mobile.Data;

public class MobileDbContext : DbContext, IApplicationDbContext
{
    public MobileDbContext(DbContextOptions<MobileDbContext> options)
        : base(options)
    {
    }

    public IQueryable<MobileUser> Users => Set<MobileUser>();
    IQueryable<Application.Common.Identity.ApplicationUser> IApplicationDbContext.Users =>
        Set<MobileUser>().Select(u => new Application.Common.Identity.ApplicationUser
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email!,
            PreferredLanguage = u.PreferredLanguage,
            TimeZone = u.TimeZone ?? "UTC"
        });

    public IQueryable<Currency> Currencies => Set<Currency>();
    public IQueryable<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public IQueryable<Category> Categories => Set<Category>();
    public IQueryable<Transaction> Transactions => Set<Transaction>();
    public IQueryable<Budget> Budgets => Set<Budget>();
    public IQueryable<Feedback> Feedbacks => Set<Feedback>();

    public new void Add<T>(T entity) where T : class => Set<T>().Add(entity);
    public new void Remove<T>(T entity) where T : class => Set<T>().Remove(entity);
    public new void Update<T>(T entity) where T : class => Set<T>().Update(entity);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Mobile User configuration (simplified - no Identity)
        builder.Entity<MobileUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.HomeCurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10);
            entity.Property(e => e.TimeZone).HasMaxLength(50);

            entity.HasIndex(e => e.Email).IsUnique();
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
            entity.Property(e => e.Rate).HasColumnType("REAL").IsRequired(); // SQLite uses REAL for decimals

            entity.HasOne(e => e.FromCurrency)
                .WithMany(c => c.FromExchangeRates)
                .HasForeignKey(e => e.FromCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToCurrency)
                .WithMany(c => c.ToExchangeRates)
                .HasForeignKey(e => e.ToCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MobileUser>()
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

            entity.HasOne<MobileUser>()
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
            entity.Property(e => e.Amount).HasColumnType("REAL").IsRequired();
            entity.Property(e => e.ConvertedAmount).HasColumnType("REAL").IsRequired();
            entity.Property(e => e.ExchangeRate).HasColumnType("REAL").IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne<MobileUser>()
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
            entity.Property(e => e.Amount).HasColumnType("REAL").IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.AlertAt80Percent).HasColumnType("REAL");
            entity.Property(e => e.AlertAt100Percent).HasColumnType("REAL");

            entity.HasOne<MobileUser>()
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
            entity.HasIndex(e => new { e.UserId, e.CategoryId, e.Period, e.IsActive });
        });

        builder.Entity<Feedback>(entity =>
        {
            entity.ToTable("Feedbacks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.AdminResponse).HasMaxLength(2000);

            entity.HasOne<MobileUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasOne<MobileUser>()
                .WithMany()
                .HasForeignKey(e => e.RespondedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
