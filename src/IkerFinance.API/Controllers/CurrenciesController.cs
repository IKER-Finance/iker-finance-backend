using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IkerFinance.Application.Common.Interfaces;

namespace IkerFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrenciesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CurrenciesController> _logger;

    public CurrenciesController(IApplicationDbContext context, ILogger<CurrenciesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveCurrencies()
    {
        var currencies = await _context.Currencies
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .Select(c => new 
            {
                c.Id,
                c.Code,
                c.Name,
                c.Symbol
            })
            .ToListAsync();

        return Ok(currencies);
    }
}