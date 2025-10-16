using IkerFinance.Domain.Entities;
using IkerFinance.Application.Common.Identity;

namespace IkerFinance.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<ApplicationUser> Users { get; }
    IQueryable<Currency> Currencies { get; }
    IQueryable<ExchangeRate> ExchangeRates { get; }
    IQueryable<Category> Categories { get; }
    IQueryable<Transaction> Transactions { get; }
    IQueryable<Budget> Budgets { get; }
    IQueryable<Feedback> Feedbacks { get; }

    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}