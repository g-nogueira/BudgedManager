using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MonthlyBudget.Integration.Tests;

[Collection("Integration")]
public sealed class ForecastApiTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    public ForecastApiTests(IntegrationTestFixture fixture) { _fixture = fixture; }

    /// <summary>
    /// Full setup: register → create household → re-login → create budget → add income → activate.
    /// Uses a unique month per call to avoid duplicate budget conflicts.
    /// </summary>
    private async Task<(HttpClient Client, Guid BudgetId)> SetupActiveBudgetAsync(string yearMonth)
    {
        var client = _fixture.CreateClient();
        var email = $"fc_{Guid.NewGuid():N}@test.com";

        // Register
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "Forecast User", password = "P@ssword123!" });

        // First login (no household)
        var tok1 = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tok1);

        // Create household
        var hResp = await client.PostAsJsonAsync("/api/v1/households", new { name = "FC Household" });
        hResp.EnsureSuccessStatusCode();

        // Re-login — now includes householdId claim
        var tok2 = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tok2);

        // Create budget
        var bResp = await client.PostAsJsonAsync("/api/v1/budgets", new { yearMonth });
        bResp.EnsureSuccessStatusCode();
        var budget = await bResp.Content.ReadFromJsonAsync<BudgetBody>();

        // Add income source
        await client.PostAsJsonAsync($"/api/v1/budgets/{budget!.BudgetId}/income",
            new { name = "Salary", amount = 5000 });

        // Activate
        var actResp = await client.PostAsync($"/api/v1/budgets/{budget.BudgetId}/activate", null);
        actResp.EnsureSuccessStatusCode();

        return (client, budget.BudgetId);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = "P@ssword123!" });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<LoginBody>();
        return body!.AccessToken;
    }

    [Fact]
    public async Task GenerateForecast_ActiveBudget_Returns201WithForecastId()
    {
        var (client, budgetId) = await SetupActiveBudgetAsync("2026-09");
        var resp = await client.PostAsync(
            $"/api/v1/budgets/{budgetId}/forecasts", null);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<ForecastBody>();
        Assert.NotEqual(Guid.Empty, body!.ForecastId);

        var detailResp = await client.GetAsync($"/api/v1/budgets/{budgetId}/forecasts/{body.ForecastId}");
        detailResp.EnsureSuccessStatusCode();
        var detail = await detailResp.Content.ReadFromJsonAsync<ForecastDetailBody>();
        Assert.Equal(5000m, detail!.StartBalance);
    }

    [Fact]
    public async Task SaveSnapshot_ExistingForecast_ReturnsIsSnapshotTrue()
    {
        var (client, budgetId) = await SetupActiveBudgetAsync("2026-10");

        var genResp = await client.PostAsync(
            $"/api/v1/budgets/{budgetId}/forecasts", null);
        genResp.EnsureSuccessStatusCode();
        var forecast = await genResp.Content.ReadFromJsonAsync<ForecastBody>();

        var snapResp = await client.PostAsync(
            $"/api/v1/budgets/{budgetId}/forecasts/{forecast!.ForecastId}/snapshot", null);
        Assert.Equal(HttpStatusCode.OK, snapResp.StatusCode);
        var snapBody = await snapResp.Content.ReadFromJsonAsync<SnapshotBody>();
        Assert.True(snapBody!.IsSnapshot);
    }

    [Fact]
    public async Task ReforecastFlow_ParentIsAutoSnapshotted()
    {
        var (client, budgetId) = await SetupActiveBudgetAsync("2026-11");

        // Generate original forecast
        var genResp = await client.PostAsync(
            $"/api/v1/budgets/{budgetId}/forecasts", null);
        genResp.EnsureSuccessStatusCode();
        var original = await genResp.Content.ReadFromJsonAsync<ForecastBody>();

        // Create reforecast — should auto-snapshot the parent
        var rfResp = await client.PostAsJsonAsync(
            $"/api/v1/budgets/{budgetId}/forecasts/{original!.ForecastId}/reforecast",
            new { startDay = 10, actualBalance = 2000m, versionLabel = "RF-1" });
        Assert.Equal(HttpStatusCode.Created, rfResp.StatusCode);

        // Verify parent is now a snapshot
        var parentResp = await client.GetAsync(
            $"/api/v1/budgets/{budgetId}/forecasts/{original.ForecastId}");
        Assert.Equal(HttpStatusCode.OK, parentResp.StatusCode);
        var parentBody = await parentResp.Content.ReadFromJsonAsync<ForecastDetailBody>();
        Assert.True(parentBody!.IsSnapshot);
    }

    [Fact]
    public async Task GetAllForecasts_AfterGenerate_ReturnsNonEmptyList()
    {
        var (client, budgetId) = await SetupActiveBudgetAsync("2026-12");
        await client.PostAsync($"/api/v1/budgets/{budgetId}/forecasts", null);
        var resp = await client.GetAsync($"/api/v1/budgets/{budgetId}/forecasts");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var list = await resp.Content.ReadFromJsonAsync<List<ForecastSummaryBody>>();
        Assert.NotEmpty(list!);
    }

    [Fact]
    public async Task CompareForecasts_TwoVersions_ReturnsDriftAnalysis()
    {
        var (client, budgetId) = await SetupActiveBudgetAsync("2027-01");

        // Generate original forecast
        var genResp1 = await client.PostAsync(
            $"/api/v1/budgets/{budgetId}/forecasts", null);
        genResp1.EnsureSuccessStatusCode();
        var forecastA = await genResp1.Content.ReadFromJsonAsync<ForecastBody>();

        // Create reforecast (auto-snapshots the parent)
        var rfResp = await client.PostAsJsonAsync(
            $"/api/v1/budgets/{budgetId}/forecasts/{forecastA!.ForecastId}/reforecast",
            new { startDay = 5, actualBalance = 2500m, versionLabel = "RF-Compare" });
        rfResp.EnsureSuccessStatusCode();
        var forecastB = await rfResp.Content.ReadFromJsonAsync<ForecastBody>();

        // Compare the two versions
        var compareResp = await client.GetAsync(
            $"/api/v1/budgets/{budgetId}/forecasts/compare?versionA={forecastA.ForecastId}&versionB={forecastB!.ForecastId}");
        Assert.Equal(HttpStatusCode.OK, compareResp.StatusCode);
        var result = await compareResp.Content.ReadFromJsonAsync<ComparisonBody>();
        Assert.NotNull(result);
        Assert.Equal(forecastA.ForecastId, result!.ForecastAId);
        Assert.Equal(forecastB.ForecastId, result.ForecastBId);
        Assert.NotEmpty(result.DayVariances);
    }

    [Fact]
    public async Task Reforecast_WithExpenseAdjustments_ModifiesEndBalance()
    {
        var (client, budgetId) = await SetupActiveBudgetAsync("2027-02");

        var genResp = await client.PostAsync(
            $"/api/v1/budgets/{budgetId}/forecasts", null);
        Assert.Equal(HttpStatusCode.Created, genResp.StatusCode);
        var original = await genResp.Content.ReadFromJsonAsync<ForecastBody>();

        var rfResp = await client.PostAsJsonAsync(
            $"/api/v1/budgets/{budgetId}/forecasts/{original!.ForecastId}/reforecast",
            new
            {
                startDay = 10,
                actualBalance = 5000m,
                versionLabel = "RF-adjust",
                expenseAdjustments = new[]
                {
                    new
                    {
                        action = "ADD",
                        newAmount = 200m,
                        name = "Car Repair",
                        category = "VARIABLE",
                        dayOfMonth = 20,
                        isSpread = false
                    }
                }
            });

        var reforecastRaw = await rfResp.Content.ReadAsStringAsync();
        Assert.True(rfResp.StatusCode == HttpStatusCode.Created,
            $"Expected Created but got {(int)rfResp.StatusCode} {rfResp.StatusCode}. Body: {reforecastRaw}");
        var reforecast = await rfResp.Content.ReadFromJsonAsync<ForecastBody>();
        Assert.True(reforecast!.EndOfMonthBalance < original.EndOfMonthBalance);
    }

    private sealed record LoginBody(string AccessToken, string RefreshToken);
    private sealed record BudgetBody(Guid BudgetId, string Status);
    private sealed record ForecastBody(Guid ForecastId, string VersionLabel, decimal EndOfMonthBalance, int DayCount);
    private sealed record SnapshotBody(Guid ForecastId, bool IsSnapshot);
    private sealed record ForecastDetailBody(Guid ForecastId, bool IsSnapshot, decimal StartBalance);
    private sealed record ForecastSummaryBody(Guid ForecastId, string VersionLabel);
    private sealed record ComparisonBody(
        Guid ForecastAId, Guid ForecastBId, string LabelA, string LabelB,
        decimal EndBalanceA, decimal EndBalanceB, decimal TotalDrift,
        List<DayVarianceBody> DayVariances, List<object> ExpenseChanges);
    private sealed record DayVarianceBody(int DayNumber, decimal BalanceA, decimal BalanceB, decimal Variance);
}
