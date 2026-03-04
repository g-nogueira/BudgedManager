using System.Security.Claims;
using Microsoft.AspNetCore.Http;
namespace MonthlyBudget.Infrastructure.Middleware;
/// <summary>
/// Validates that the authenticated user has a householdId claim.
/// All business operations must be scoped to a household.
/// Certain endpoints (household creation, joining) are exempt.
/// </summary>
public sealed class HouseholdScopeMiddleware
{
    private readonly RequestDelegate _next;

    // Paths that don't require a householdId (user is establishing their household context)
    private static readonly string[] _exemptPrefixes =
    {
        "/api/v1/households",  // POST /api/v1/households (create) + GET (read) + POST /join
        "/api/v1/auth",
        "/swagger",
        "/health"
    };

    public HouseholdScopeMiddleware(RequestDelegate next) { _next = next; }
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var isExempt = _exemptPrefixes.Any(p =>
                path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            if (!isExempt)
            {
                var householdId = context.User.FindFirstValue("householdId");
                if (string.IsNullOrEmpty(householdId))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { error = "User does not belong to a household." });
                    return;
                }
            }
        }
        await _next(context);
    }
}
