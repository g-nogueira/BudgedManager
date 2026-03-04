using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonthlyBudget.ForecastEngine.Application.Features.CompareForecasts;
using MonthlyBudget.ForecastEngine.Application.Features.GenerateForecast;
using MonthlyBudget.ForecastEngine.Application.Features.GetForecast;
using MonthlyBudget.ForecastEngine.Application.Features.Reforecast;
using MonthlyBudget.ForecastEngine.Application.Features.SaveSnapshot;
using MonthlyBudget.ForecastEngine.Infrastructure.Dto;
using System.Security.Claims;
namespace MonthlyBudget.ForecastEngine.Infrastructure.Controllers;
[ApiController]
[Route("api/v1/budgets/{budgetId:guid}/forecasts")]
[Authorize]
public sealed class ForecastController : ControllerBase
{
    private readonly IMediator _mediator;
    public ForecastController(IMediator mediator) { _mediator = mediator; }
    private Guid HouseholdId => Guid.Parse(User.FindFirstValue("householdId")!);
    [HttpPost]
    public async Task<IActionResult> Generate(Guid budgetId, [FromBody] GenerateForecastRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateForecastCommand(budgetId, HouseholdId, req.StartBalance), ct);
        return CreatedAtAction(nameof(GetById), new { budgetId, forecastId = result.ForecastId }, result);
    }
    [HttpGet("{forecastId:guid}")]
    public async Task<IActionResult> GetById(Guid budgetId, Guid forecastId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetForecastQuery(forecastId, HouseholdId), ct);
        return result == null ? NotFound() : Ok(result);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid budgetId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetForecastsByBudgetQuery(budgetId, HouseholdId), ct);
        return Ok(result);
    }
    [HttpGet("compare")]
    public async Task<IActionResult> Compare(Guid budgetId, [FromQuery] Guid versionA, [FromQuery] Guid versionB, CancellationToken ct)
    {
        var result = await _mediator.Send(new CompareForecastsQuery(versionA, versionB, HouseholdId), ct);
        return Ok(result);
    }
    [HttpPost("{forecastId:guid}/snapshot")]
    public async Task<IActionResult> SaveSnapshot(Guid budgetId, Guid forecastId, CancellationToken ct)
    {
        var result = await _mediator.Send(new SaveSnapshotCommand(forecastId, HouseholdId), ct);
        return Ok(result);
    }
    [HttpPost("{forecastId:guid}/reforecast")]
    public async Task<IActionResult> Reforecast(Guid budgetId, Guid forecastId, [FromBody] ReforecastRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReforecastCommand(budgetId, HouseholdId, forecastId, req.StartDay, req.ActualBalance, req.VersionLabel), ct);
        return Ok(result);
    }
}
