using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonthlyBudget.BudgetManagement.Application.Features.AddExpense;
using MonthlyBudget.BudgetManagement.Application.Features.RemoveExpense;
using MonthlyBudget.BudgetManagement.Application.Features.ToggleExpenseExclusion;
using MonthlyBudget.BudgetManagement.Application.Features.UpdateExpense;
using MonthlyBudget.BudgetManagement.Infrastructure.Dto;
using System.Security.Claims;
namespace MonthlyBudget.BudgetManagement.Infrastructure.Controllers;
[ApiController]
[Route("api/v1/budgets/{budgetId:guid}/expenses")]
[Authorize]
public sealed class ExpenseController : ControllerBase
{
    private readonly IMediator _mediator;
    public ExpenseController(IMediator mediator) { _mediator = mediator; }
    private Guid HouseholdId => Guid.Parse(User.FindFirstValue("householdId")!);
    [HttpPost]
    public async Task<IActionResult> Add(Guid budgetId, [FromBody] AddExpenseRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddExpenseCommand(budgetId, HouseholdId, req.Name, req.Category, req.DayOfMonth, req.IsSpread, req.Amount), ct);
        return Created($"api/v1/budgets/{budgetId}/expenses/{result.ExpenseId}", result);
    }
    [HttpPut("{expenseId:guid}")]
    public async Task<IActionResult> Update(Guid budgetId, Guid expenseId, [FromBody] UpdateExpenseRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateExpenseCommand(budgetId, HouseholdId, expenseId, req.Name, req.Category, req.DayOfMonth, req.IsSpread, req.Amount), ct);
        return NoContent();
    }
    [HttpDelete("{expenseId:guid}")]
    public async Task<IActionResult> Remove(Guid budgetId, Guid expenseId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveExpenseCommand(budgetId, HouseholdId, expenseId), ct);
        return NoContent();
    }
    [HttpPatch("{expenseId:guid}/exclusion")]
    public async Task<IActionResult> ToggleExclusion(Guid budgetId, Guid expenseId, [FromBody] ToggleExclusionRequest req, CancellationToken ct)
    {
        await _mediator.Send(new ToggleExpenseExclusionCommand(budgetId, HouseholdId, expenseId, req.IsExcluded), ct);
        return NoContent();
    }
}
