using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonthlyBudget.BudgetManagement.Application.Features.ActivateBudget;
using MonthlyBudget.BudgetManagement.Application.Features.CloseBudget;
using MonthlyBudget.BudgetManagement.Application.Features.CreateBudget;
using MonthlyBudget.BudgetManagement.Application.Features.GetBudget;
using MonthlyBudget.BudgetManagement.Application.Features.RolloverMonth;
using MonthlyBudget.BudgetManagement.Infrastructure.Dto;
using System.Security.Claims;
namespace MonthlyBudget.BudgetManagement.Infrastructure.Controllers;
[ApiController]
[Route("api/v1/budgets")]
[Authorize]
public sealed class BudgetController : ControllerBase
{
    private readonly IMediator _mediator;
    public BudgetController(IMediator mediator) { _mediator = mediator; }
    private Guid HouseholdId => Guid.Parse(User.FindFirstValue("householdId")!);
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateBudgetCommand(HouseholdId, req.YearMonth), ct);
        return CreatedAtAction(nameof(GetById), new { budgetId = result.BudgetId }, result);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBudgetsByHouseholdQuery(HouseholdId), ct);
        return Ok(result);
    }
    [HttpGet("{budgetId:guid}")]
    public async Task<IActionResult> GetById(Guid budgetId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBudgetByIdQuery(budgetId, HouseholdId), ct);
        return result == null ? NotFound() : Ok(result);
    }
    [HttpGet("by-month/{yearMonth}")]
    public async Task<IActionResult> GetByMonth(string yearMonth, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBudgetByMonthQuery(HouseholdId, yearMonth), ct);
        return result == null ? NotFound() : Ok(result);
    }
    [HttpPost("{budgetId:guid}/rollover")]
    public async Task<IActionResult> Rollover(Guid budgetId, [FromBody] RolloverMonthRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RolloverMonthCommand(budgetId, HouseholdId, req.TargetYearMonth), ct);
        return Ok(result);
    }
    [HttpPost("{budgetId:guid}/activate")]
    public async Task<IActionResult> Activate(Guid budgetId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ActivateBudgetCommand(budgetId, HouseholdId), ct);
        return Ok(result);
    }

    [HttpPost("{budgetId:guid}/close")]
    public async Task<IActionResult> Close(Guid budgetId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CloseBudgetCommand(budgetId, HouseholdId), ct);
        return Ok(result);
    }
}
