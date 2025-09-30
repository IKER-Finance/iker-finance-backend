using Microsoft.EntityFrameworkCore;
using IkerFinance.Domain.Entities;

namespace IkerFinance.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<ExchangeRate> ExchangeRates { get; }
    DbSet<Category> Categories { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Budget> Budgets { get; }
    DbSet<BudgetCategory> BudgetCategories { get; }
    DbSet<Feedback> Feedbacks { get; }
    DbSet<Export> Exports { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}