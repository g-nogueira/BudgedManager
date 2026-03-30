using MonthlyBudget.IdentityHousehold.Domain.Entities;
namespace MonthlyBudget.IdentityHousehold.Domain.Repositories;
public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task SaveAsync(User user, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default);
}
public interface IHouseholdRepository
{
    Task<Household?> FindByIdAsync(Guid householdId, CancellationToken ct = default);
    Task<Household?> FindByMemberIdAsync(Guid userId, CancellationToken ct = default);
    Task SaveAsync(Household household, CancellationToken ct = default);
}
public interface IInvitationRepository
{
    Task<Invitation?> FindByTokenAsync(string token, CancellationToken ct = default);
    Task<Invitation?> FindByHouseholdAndEmailAsync(Guid householdId, string email, CancellationToken ct = default);
    Task<Invitation?> FindPendingByHouseholdAsync(Guid householdId, CancellationToken ct = default);
    Task<IReadOnlyList<Invitation>> FindAllExpiredPendingAsync(CancellationToken ct = default);
    Task SaveAsync(Invitation invitation, CancellationToken ct = default);
    Task SaveAllAsync(IEnumerable<Invitation> invitations, CancellationToken ct = default);
}

public interface IRefreshTokenRepository
{
    Task<RefreshTokenEntry?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task SaveAsync(RefreshTokenEntry refreshToken, CancellationToken ct = default);
    Task DeleteAsync(RefreshTokenEntry refreshToken, CancellationToken ct = default);
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken ct = default);
}
