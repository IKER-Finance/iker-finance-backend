using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Services;
using IkerFinance.Shared.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Commands.UpdateBudget;

public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, BudgetDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;
    private readonly BudgetService _budgetService;

    public UpdateBudgetCommandHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService)
    {
        _context = context;
        _conversionService = conversionService;
        _budgetService = new BudgetService();
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

        var currency = await _context.Currencies.FindAsync(new object[] { request.CurrencyId }, cancellationToken);
        if (currency == null || !currency.IsActive)
            throw new ValidationException("Invalid or inactive currency");

        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        var homeCurrencyId = user!.HomeCurrencyId!.Value;

        if (request.CurrencyId != homeCurrencyId)
        {
            var rateExists = await _conversionService.RatesExistAsync(request.CurrencyId, homeCurrencyId);
            if (!rateExists)
                throw new ValidationException("Budget currency needs exchange rate to home currency");
        }

        if (request.CategoryAllocations.Any())
        {
            var categoryIds = request.CategoryAllocations.Select(a => a.CategoryId).ToList();
            var categories = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            if (categories.Count != categoryIds.Count)
                throw new NotFoundException("One or more categories not found");
        }

        _budgetService.Update(
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
            _context.BudgetCategories.Remove(existing);
        }

        foreach (var allocation in request.CategoryAllocations)
        {
            var budgetCategory = new BudgetCategory
            {
                BudgetId = budget.Id,
                CategoryId = allocation.CategoryId,
                Amount = allocation.Amount
            };
            _context.BudgetCategories.Add(budgetCategory);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updatedBudget = await _context.Budgets
            .Include(b => b.Currency)
            .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
            .FirstAsync(b => b.Id == budget.Id, cancellationToken);

        return new BudgetDto
        {
            Id = updatedBudget.Id,
            Name = updatedBudget.Name,
            Period = updatedBudget.Period,
            StartDate = updatedBudget.StartDate,
            EndDate = updatedBudget.EndDate,
            Amount = updatedBudget.Amount,
            CurrencyId = updatedBudget.CurrencyId,
            CurrencyCode = currency.Code,
            CurrencySymbol = currency.Symbol,
            IsActive = updatedBudget.IsActive,
            Description = updatedBudget.Description,
            AllowOverlap = updatedBudget.AllowOverlap,
            AlertAt80Percent = updatedBudget.AlertAt80Percent,
            AlertAt100Percent = updatedBudget.AlertAt100Percent,
            AlertsEnabled = updatedBudget.AlertsEnabled,
            Categories = updatedBudget.Categories.Select(bc => new BudgetCategoryDto
            {
                CategoryId = bc.CategoryId,
                CategoryName = bc.Category.Name,
                Amount = bc.Amount
            }).ToList(),
            CreatedAt = updatedBudget.CreatedAt
        };
    }
}