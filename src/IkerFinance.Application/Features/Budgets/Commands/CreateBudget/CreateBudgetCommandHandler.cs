using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.Services;
using IkerFinance.Shared.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Commands.CreateBudget;

public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, BudgetDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;
    private readonly BudgetService _budgetService;

    public CreateBudgetCommandHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService)
    {
        _context = context;
        _conversionService = conversionService;
        _budgetService = new BudgetService();
    }

    public async Task<BudgetDto> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null || !user.HomeCurrencyId.HasValue)
            throw new NotFoundException("User", request.UserId);

        var currency = await _context.Currencies.FindAsync(new object[] { request.CurrencyId }, cancellationToken);
        if (currency == null || !currency.IsActive)
            throw new ValidationException("Invalid or inactive currency");

        var homeCurrencyId = user.HomeCurrencyId.Value;

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

        var budget = _budgetService.Create(
            userId: request.UserId,
            name: request.Name,
            currencyId: request.CurrencyId,
            amount: request.Amount,
            period: request.Period,
            startDate: request.StartDate,
            description: request.Description
        );

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync(cancellationToken);

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