using System.Security.Claims;
using Microsoft.AspNetCore.Http;
namespace MonthlyBudget.Infrastructure.Middleware;
/// <summary>
/// Validates that the authenticated user has a householdId claim.
/// All business operations must be scoped to a household.
/// </summary>
public sealed class HouseholdScopeMiddleware
{
    private readonly RequestDelegate _next;
    public HouseholdScopeMiddleware(RequestDelegate next) { _next = next; }
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var householdId = context.User.FindFirstValue("householdId");
            if (string.IsNullOrEmpty(householdId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "User does not belong to a household." });
                return;
            }
        }
        await _next(context);
    }
}
