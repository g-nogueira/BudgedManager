using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonthlyBudget.BudgetManagement.Application.Features.AddIncome;
using MonthlyBudget.BudgetManagement.Application.Features.RemoveIncome;
using MonthlyBudget.BudgetManagement.Application.Features.UpdateIncome;
using MonthlyBudget.BudgetManagement.Infrastructure.Dto;
using System.Security.Claims;
namespace MonthlyBudget.BudgetManagement.Infrastructure.Controllers;
[ApiController]
[Route("api/v1/budgets/{budgetId:guid}/incomes")]
[Authorize]
public sealed class IncomeController : ControllerBase
{
    private readonly IMediator _mediator;
    public IncomeController(IMediator mediator) { _mediator = mediator; }
    private Guid HouseholdId => Guid.Parse(User.FindFirstValue("householdId")!);
    [HttpPost]
    public async Task<IActionResult> Add(Guid budgetId, [FromBody] AddIncomeRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddIncomeCommand(budgetId, HouseholdId, req.Name, req.Amount), ct);
        return Created($"api/v1/budgets/{budgetId}/incomes/{result.IncomeId}", result);
    }
    [HttpPut("{incomeId:guid}")]
    public async Task<IActionResult> Update(Guid budgetId, Guid incomeId, [FromBody] UpdateIncomeRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateIncomeCommand(budgetId, HouseholdId, incomeId, req.Name, req.Amount), ct);
        return NoContent();
    }
    [HttpDelete("{incomeId:guid}")]
    public async Task<IActionResult> Remove(Guid budgetId, Guid incomeId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveIncomeCommand(budgetId, HouseholdId, incomeId), ct);
        return NoContent();
    }
}
