namespace MonthlyBudget.IdentityHousehold.Application.Ports;
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string displayName, Guid? householdId);
    string GenerateRefreshToken();
}
public interface IEmailService
{
    Task SendInvitationAsync(string toEmail, string householdName, string token, CancellationToken ct = default);
}
