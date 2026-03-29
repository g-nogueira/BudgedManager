using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MonthlyBudget.Integration.Tests;

[Collection("Integration")]
public sealed class BudgetManagementApiTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    public BudgetManagementApiTests(IntegrationTestFixture fixture) { _fixture = fixture; }

    // ── Helpers ─────────────────────────────────────────────────────────────────
    /// <summary>Register, create household, re-login → returns token with householdId claim.</summary>
    private async Task<(HttpClient Client, Guid BudgetId)> SetupWithBudgetAsync(string yearMonth)
    {
        var client = _fixture.CreateClient();
        var email = $"bm_{Guid.NewGuid():N}@test.com";

        // 1 – Register
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "Test Owner", password = "P@ssword123!" });

        // 2 – First login (no household yet)
        var tok1 = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tok1);

        // 3 – Create household
        var hResp = await client.PostAsJsonAsync("/api/v1/households", new { name = "Test Household" });
        hResp.EnsureSuccessStatusCode();

        // 4 – Re-login — now includes householdId claim
        var tok2 = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tok2);

        // 5 – Create budget
        var bResp = await client.PostAsJsonAsync("/api/v1/budgets", new { yearMonth });
        bResp.EnsureSuccessStatusCode();
        var budget = await bResp.Content.ReadFromJsonAsync<BudgetBody>();
        return (client, budget!.BudgetId);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = "P@ssword123!" });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<LoginBody>();
        return body!.AccessToken;
    }

    // ── Tests ────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Register_ValidData_Returns201()
    {
        var client = _fixture.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = $"reg_{Guid.NewGuid():N}@test.com",
            displayName = "Test User",
            password = "P@ssword123!"
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var client = _fixture.CreateClient();
        var email = $"login_{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "Login User", password = "P@ssword123!" });
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = "P@ssword123!" });
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<LoginBody>();
        Assert.False(string.IsNullOrEmpty(body?.AccessToken));
    }

    [Fact]
    public async Task CreateHousehold_AuthenticatedUser_Returns201()
    {
        var client = _fixture.CreateClient();
        var email = $"hh_{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "HH Owner", password = "P@ssword123!" });
        var tok = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tok);
        var resp = await client.PostAsJsonAsync("/api/v1/households", new { name = "My Household" });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task CreateBudget_WithHouseholdToken_Returns201()
    {
        var (client, budgetId) = await SetupWithBudgetAsync("2026-06");
        Assert.NotEqual(Guid.Empty, budgetId);
    }

    [Fact]
    public async Task ListBudgets_WithHouseholdToken_Returns200WithSummaries()
    {
        var (client, budgetId) = await SetupWithBudgetAsync("2026-11");

        var response = await client.GetAsync("/api/v1/budgets");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summaries = await response.Content.ReadFromJsonAsync<BudgetSummaryBody[]>();
        Assert.NotNull(summaries);

        var matching = summaries!.FirstOrDefault(x => x.BudgetId == budgetId);
        Assert.NotNull(matching);
        Assert.Equal("2026-11", matching!.YearMonth);
        Assert.Equal("DRAFT", matching.Status);
    }

    [Fact]
    public async Task ActivateBudget_WithIncomeSources_Returns200Active()
    {
        var (client, budgetId) = await SetupWithBudgetAsync("2026-07");

        // Add income source
        var incResp = await client.PostAsJsonAsync(
            $"/api/v1/budgets/{budgetId}/income", new { name = "Salary", amount = 5000 });
        incResp.EnsureSuccessStatusCode();

        // Activate
        var actResp = await client.PostAsync($"/api/v1/budgets/{budgetId}/activate", null);
        Assert.Equal(HttpStatusCode.OK, actResp.StatusCode);
        var result = await actResp.Content.ReadFromJsonAsync<ActivateBody>();
        Assert.Equal("ACTIVE", result!.Status);
    }

    [Fact]
    public async Task DuplicateBudget_SameMonth_Returns400()
    {
        var (client, _) = await SetupWithBudgetAsync("2026-08");
        // Second budget for same month should fail
        var dup = await client.PostAsJsonAsync("/api/v1/budgets", new { yearMonth = "2026-08" });
        Assert.Equal(HttpStatusCode.BadRequest, dup.StatusCode);
    }

    // ── Response bodies ───────────────────────────────────────────────────────────
    private sealed record LoginBody(string AccessToken, string RefreshToken);
    private sealed record BudgetBody(Guid BudgetId, string Status);
    private sealed record BudgetSummaryBody(Guid BudgetId, string YearMonth, string Status, decimal TotalIncome, decimal TotalExpenses, DateTime CreatedAt);
    private sealed record ActivateBody(Guid BudgetId, string Status);
}
