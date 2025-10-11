using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.DomainServices.Budget;
using IkerFinance.Application.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, BudgetDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;
    private readonly BudgetUpdater _budgetUpdater;

    public UpdateBudgetCommandHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService,
        BudgetUpdater budgetUpdater)
    {
        _context = context;
        _conversionService = conversionService;
        _budgetUpdater = budgetUpdater;
    }

    public async Task<BudgetDto> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .Where(b => b.Id == request.Id && b.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (budget == null)
            throw new NotFoundException("Budget", request.Id);

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.CurrencyId, cancellationToken);
        if (currency == null || !currency.IsActive)
            throw new ValidationException("Invalid or inactive currency");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        var homeCurrencyId = user!.HomeCurrencyId!.Value;

        if (request.CurrencyId != homeCurrencyId)
        {
            var rateExists = await _conversionService.RatesExistAsync(request.CurrencyId, homeCurrencyId);
            if (!rateExists)
                throw new ValidationException("Budget currency needs exchange rate to home currency");
        }

        List<Category> categories = new();
        if (request.CategoryAllocations.Any())
        {
            var categoryIds = request.CategoryAllocations.Select(a => a.CategoryId).ToList();
            categories = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            if (categories.Count != categoryIds.Count)
                throw new NotFoundException("One or more categories not found");
        }

        _budgetUpdater.Update(
            budget: budget,
            name: request.Name,
            currencyId: request.CurrencyId,
            amount: request.Amount,
            period: request.Period,
            startDate: request.StartDate,
            description: request.Description,
            isActive: request.IsActive
        );

        var existingCategories = budget.Categories.ToList();
        foreach (var existing in existingCategories)
        {
            _context.Remove(existing);
        }

        foreach (var allocation in request.CategoryAllocations)
        {
            var budgetCategory = new BudgetCategory
            {
                BudgetId = budget.Id,
                CategoryId = allocation.CategoryId,
                Amount = allocation.Amount
            };
            _context.Add(budgetCategory);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new BudgetDto
        {
            Id = budget.Id,
            Name = budget.Name,
            Period = budget.Period,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            Amount = budget.Amount,
            CurrencyId = budget.CurrencyId,
            CurrencyCode = currency.Code,
            CurrencySymbol = currency.Symbol,
            IsActive = budget.IsActive,
            Description = budget.Description,
            AllowOverlap = budget.AllowOverlap,
            AlertAt80Percent = budget.AlertAt80Percent,
            AlertAt100Percent = budget.AlertAt100Percent,
            AlertsEnabled = budget.AlertsEnabled,
            Categories = request.CategoryAllocations.Select(a => new BudgetCategoryDto
            {
                CategoryId = a.CategoryId,
                CategoryName = categories.First(c => c.Id == a.CategoryId).Name,
                Amount = a.Amount
            }).ToList(),
            CreatedAt = budget.CreatedAt
        };
    }
}