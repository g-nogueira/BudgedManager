using Microsoft.EntityFrameworkCore;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
using MonthlyBudget.Infrastructure.Database;
namespace MonthlyBudget.Infrastructure.Repositories;

public sealed class PostgresUserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public PostgresUserRepository(AppDbContext db) { _db = db; }
    public async Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Users.FindAsync(new object[] { userId }, ct);
    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);
    public async Task SaveAsync(User user, CancellationToken ct = default)
    {
        var existing = await _db.Users.FindAsync(new object[] { user.UserId }, ct);
        if (existing == null) _db.Users.Add(user);
        else _db.Entry(existing).CurrentValues.SetValues(user);
        await _db.SaveChangesAsync(ct);
    }
    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
        => await _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);
}

public sealed class PostgresHouseholdRepository : IHouseholdRepository
{
    private readonly AppDbContext _db;
    public PostgresHouseholdRepository(AppDbContext db) { _db = db; }

    public async Task<Household?> FindByIdAsync(Guid householdId, CancellationToken ct = default)
        => await _db.Households.Include(h => h.Members)
            .FirstOrDefaultAsync(h => h.HouseholdId == householdId, ct);

    public async Task<Household?> FindByMemberIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Households.Include(h => h.Members)
            .FirstOrDefaultAsync(h => h.Members.Any(m => m.UserId == userId), ct);

    public async Task SaveAsync(Household household, CancellationToken ct = default)
    {
        var existing = await _db.Households
            .FindAsync(new object[] { household.HouseholdId }, ct);
        if (existing == null)
        {
            _db.Households.Add(household);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(household);
            // Track newly added Members — private backing field bypasses EF proxy interception
            foreach (var member in household.Members)
            {
                var entry = _db.Entry(member);
                if (entry.State == EntityState.Detached)
                {
                    _db.Entry(member).Property("HouseholdId").CurrentValue = household.HouseholdId;
                    entry.State = EntityState.Added;
                }
            }
        }
        await _db.SaveChangesAsync(ct);
    }
}

public sealed class PostgresInvitationRepository : IInvitationRepository
{
    private readonly AppDbContext _db;
    public PostgresInvitationRepository(AppDbContext db) { _db = db; }

    public async Task<Invitation?> FindByTokenAsync(string token, CancellationToken ct = default)
        => await _db.Invitations.FirstOrDefaultAsync(i => i.Token == token, ct);

    public async Task<Invitation?> FindByHouseholdAndEmailAsync(Guid householdId, string email, CancellationToken ct = default)
        => await _db.Invitations.FirstOrDefaultAsync(
            i => i.HouseholdId == householdId && i.InvitedEmail == email.ToLowerInvariant(), ct);

    public async Task<Invitation?> FindPendingByHouseholdAsync(Guid householdId, CancellationToken ct = default)
        => await _db.Invitations.FirstOrDefaultAsync(
            i => i.HouseholdId == householdId && i.Status == InvitationStatus.PENDING, ct);

    public async Task SaveAsync(Invitation invitation, CancellationToken ct = default)
    {
        var existing = await _db.Invitations
            .FindAsync(new object[] { invitation.InvitationId }, ct);
        if (existing == null) _db.Invitations.Add(invitation);
        else _db.Entry(existing).CurrentValues.SetValues(invitation);
        await _db.SaveChangesAsync(ct);
    }
}
