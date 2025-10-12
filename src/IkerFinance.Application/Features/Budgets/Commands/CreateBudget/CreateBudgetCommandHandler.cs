using MediatR;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;
using IkerFinance.Application.Common.Exceptions;
using IkerFinance.Domain.Entities;
using IkerFinance.Domain.DomainServices.Budget;
using IkerFinance.Application.DTOs.Budgets;

namespace IkerFinance.Application.Features.Budgets.Commands.CreateBudget;

public sealed class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, BudgetDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyConversionService _conversionService;
    private readonly BudgetService _budgetService;

    public CreateBudgetCommandHandler(
        IApplicationDbContext context,
        ICurrencyConversionService conversionService,
        BudgetService budgetService)
    {
        _context = context;
        _conversionService = conversionService;
        _budgetService = budgetService;
    }

    public async Task<BudgetDto> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null || !user.HomeCurrencyId.HasValue)
            throw new NotFoundException("User", request.UserId);

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.CurrencyId, cancellationToken);
        if (currency == null || !currency.IsActive)
            throw new ValidationException("Invalid or inactive currency");

        var homeCurrencyId = user.HomeCurrencyId.Value;

        if (request.CurrencyId != homeCurrencyId)
        {
            var rateExists = await _conversionService.RatesExistAsync(request.CurrencyId, homeCurrencyId);
            if (!rateExists)
                throw new ValidationException("Budget currency needs exchange rate to home currency");
        }

        // Validate category exists and is active
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);
        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        // Check for existing active budget for same category + period
        var endDate = _budgetService.CalculateEndDate(request.StartDate, request.Period);
        var existingBudget = await _context.Budgets
            .AnyAsync(b =>
                b.UserId == request.UserId &&
                b.CategoryId == request.CategoryId &&
                b.Period == request.Period &&
                b.IsActive &&
                ((b.StartDate <= request.StartDate && b.EndDate >= request.StartDate) ||
                 (b.StartDate <= endDate && b.EndDate >= endDate) ||
                 (b.StartDate >= request.StartDate && b.EndDate <= endDate)),
                cancellationToken);

        if (existingBudget)
            throw new ValidationException($"An active {request.Period} budget already exists for category '{category.Name}'");

        var budget = _budgetService.Create(
            userId: request.UserId,
            categoryId: request.CategoryId,
            currencyId: request.CurrencyId,
            amount: request.Amount,
            period: request.Period,
            startDate: request.StartDate,
            description: request.Description
        );

        _context.Add(budget);
        await _context.SaveChangesAsync(cancellationToken);

        // Load with includes for response
        budget = await _context.Budgets
            .Include(b => b.Category)
            .Include(b => b.Currency)
            .FirstAsync(b => b.Id == budget.Id, cancellationToken);

        return new BudgetDto
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category.Name,
            CategoryIcon = budget.Category.Icon,
            CategoryColor = budget.Category.Color,
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
            CreatedAt = budget.CreatedAt
        };
    }
}