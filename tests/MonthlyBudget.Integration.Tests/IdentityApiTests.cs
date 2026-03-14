using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MonthlyBudget.Integration.Tests;

[Collection("Integration")]
public sealed class IdentityApiTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    public IdentityApiTests(IntegrationTestFixture fixture) { _fixture = fixture; }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = "P@ssword123!" });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<LoginBody>();
        return body!.AccessToken;
    }

    private async Task<(HttpClient Client, Guid HouseholdId)> RegisterAndCreateHouseholdAsync(string emailPrefix)
    {
        var client = _fixture.CreateClient();
        var email = $"{emailPrefix}_{Guid.NewGuid():N}@test.com";

        await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "Test Owner", password = "P@ssword123!" });

        var tok1 = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tok1);

        var hResp = await client.PostAsJsonAsync("/api/v1/households", new { name = "Test Household" });
        hResp.EnsureSuccessStatusCode();
        var household = await hResp.Content.ReadFromJsonAsync<HouseholdCreatedBody>();

        // Re-login to get householdId claim
        var tok2 = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tok2);

        return (client, household!.HouseholdId);
    }

    // ── Story #19: User Registration & Authentication ─────────────────────────────

    [Fact]
    public async Task Register_DuplicateEmail_Returns409Conflict()
    {
        var client = _fixture.CreateClient();
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "First", password = "P@ssword123!" });

        var resp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "Second", password = "P@ssword123!" });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmailFormat_Returns422()
    {
        var client = _fixture.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email = "not-an-email", displayName = "Test", password = "P@ssword123!" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns422()
    {
        var client = _fixture.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email = $"pw_{Guid.NewGuid():N}@test.com", displayName = "Test", password = "short" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = _fixture.CreateClient();
        var email = $"wp_{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "Test", password = "P@ssword123!" });

        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var client = _fixture.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "nobody@example.com", password = "P@ssword123!" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Story #43: Household Creation & Management ───────────────────────────────

    [Fact]
    public async Task GetHousehold_AsMember_ReturnsHouseholdWithMembers()
    {
        var (client, householdId) = await RegisterAndCreateHouseholdAsync("gethh");

        var resp = await client.GetAsync($"/api/v1/households/{householdId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<HouseholdDto>();
        Assert.NotNull(body);
        Assert.Equal(householdId, body!.HouseholdId);
        Assert.NotEmpty(body.Members);
        Assert.Contains(body.Members, m => m.Role == "OWNER");
    }

    [Fact]
    public async Task CreateHousehold_UserAlreadyInHousehold_Returns409()
    {
        var (client, _) = await RegisterAndCreateHouseholdAsync("duplhh");

        // Second attempt
        var resp = await client.PostAsJsonAsync("/api/v1/households", new { name = "Second Household" });
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    // ── Story #44: Member Invitation & Join Flow ──────────────────────────────────

    [Fact]
    public async Task InviteMember_AsOwner_Returns201WithInvitationId()
    {
        var (client, householdId) = await RegisterAndCreateHouseholdAsync("inv_owner");
        var partnerEmail = $"partner_{Guid.NewGuid():N}@test.com";

        var resp = await client.PostAsJsonAsync(
            $"/api/v1/households/{householdId}/invite",
            new { email = partnerEmail });

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<InvitationBody>();
        Assert.NotEqual(Guid.Empty, body!.InvitationId);
    }

    [Fact]
    public async Task InviteMember_PendingAlreadyExists_Returns409()
    {
        var (client, householdId) = await RegisterAndCreateHouseholdAsync("inv_dup");
        var email1 = $"pe1_{Guid.NewGuid():N}@test.com";
        var email2 = $"pe2_{Guid.NewGuid():N}@test.com";

        // First invite
        await client.PostAsJsonAsync(
            $"/api/v1/households/{householdId}/invite", new { email = email1 });

        // Second invite — should conflict (INV-H4)
        var resp = await client.PostAsJsonAsync(
            $"/api/v1/households/{householdId}/invite", new { email = email2 });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task JoinHousehold_ValidToken_Returns200WithHouseholdId()
    {
        // Setup: owner creates household and invites partner
        var (ownerClient, householdId) = await RegisterAndCreateHouseholdAsync("join_owner");
        var partnerEmail = $"partner_{Guid.NewGuid():N}@test.com";

        var invResp = await ownerClient.PostAsJsonAsync(
            $"/api/v1/households/{householdId}/invite",
            new { email = partnerEmail });
        invResp.EnsureSuccessStatusCode();
        var invitation = await invResp.Content.ReadFromJsonAsync<InvitationBody>();
        var invToken = await _fixture.GetInvitationTokenAsync(invitation!.InvitationId);

        // Partner registers
        var partnerClient = _fixture.CreateClient();
        await partnerClient.PostAsJsonAsync("/api/v1/auth/register",
            new { email = partnerEmail, displayName = "Partner", password = "P@ssword123!" });
        var partnerTok = await LoginAsync(partnerClient, partnerEmail);
        partnerClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", partnerTok);

        // Partner joins
        var joinResp = await partnerClient.PostAsJsonAsync(
            "/api/v1/households/join", new { token = invToken });

        Assert.Equal(HttpStatusCode.OK, joinResp.StatusCode);
        var joinBody = await joinResp.Content.ReadFromJsonAsync<JoinBody>();
        Assert.Equal(householdId, joinBody!.HouseholdId);
    }

    [Fact]
    public async Task JoinHousehold_InvalidToken_Returns404()
    {
        var client = _fixture.CreateClient();
        var email = $"join404_{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email, displayName = "User", password = "P@ssword123!" });
        var tok = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tok);

        var resp = await client.PostAsJsonAsync("/api/v1/households/join",
            new { token = "completely-invalid-token-xyz" });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task InviteMember_HouseholdFull_Returns409()
    {
        // Create owner's household
        var (ownerClient, householdId) = await RegisterAndCreateHouseholdAsync("inv_full_owner");
        var partnerEmail = $"partner_full_{Guid.NewGuid():N}@test.com";

        // Invite partner
        var invResp = await ownerClient.PostAsJsonAsync(
            $"/api/v1/households/{householdId}/invite",
            new { email = partnerEmail });
        invResp.EnsureSuccessStatusCode();
        var invitation = await invResp.Content.ReadFromJsonAsync<InvitationBody>();

        // Partner registers and joins
        var partnerClient = _fixture.CreateClient();
        await partnerClient.PostAsJsonAsync("/api/v1/auth/register",
            new { email = partnerEmail, displayName = "Partner", password = "P@ssword123!" });
        var partnerTok = await LoginAsync(partnerClient, partnerEmail);
        partnerClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", partnerTok);
        var invToken2 = await _fixture.GetInvitationTokenAsync(invitation!.InvitationId);
        var joinResp = await partnerClient.PostAsJsonAsync(
            "/api/v1/households/join", new { token = invToken2 });
        joinResp.EnsureSuccessStatusCode();

        // Now household has 2 members — invite a 3rd should be 409 (INV-H1)
        var thirdEmail = $"third_{Guid.NewGuid():N}@test.com";
        var fullResp = await ownerClient.PostAsJsonAsync(
            $"/api/v1/households/{householdId}/invite",
            new { email = thirdEmail });

        Assert.Equal(HttpStatusCode.Conflict, fullResp.StatusCode);
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────────

    private sealed record LoginBody(string AccessToken, string RefreshToken, Guid UserId, Guid? HouseholdId);
    private sealed record HouseholdCreatedBody(Guid HouseholdId, string Name);
    private sealed record HouseholdDto(Guid HouseholdId, string Name, List<MemberDto> Members);
    private sealed record MemberDto(Guid UserId, string Role);
    private sealed record InvitationBody(Guid InvitationId);
    private sealed record JoinBody(Guid HouseholdId);
}


