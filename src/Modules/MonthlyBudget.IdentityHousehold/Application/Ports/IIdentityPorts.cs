using MonthlyBudget.SharedKernel.Events;
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
    string HashToken(string token);
}
public interface IEmailService
{
    Task SendInvitationAsync(string toEmail, string householdName, string token, CancellationToken ct = default);
}
public interface IHouseholdEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
