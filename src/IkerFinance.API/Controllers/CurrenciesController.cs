using Microsoft.AspNetCore.Mvc;
using MediatR;
using IkerFinance.Application.Features.Currencies.Queries.GetCurrencies;

namespace IkerFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrenciesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CurrenciesController> _logger;

    public CurrenciesController(IMediator mediator, ILogger<CurrenciesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveCurrencies()
    {
        var query = new GetCurrenciesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}