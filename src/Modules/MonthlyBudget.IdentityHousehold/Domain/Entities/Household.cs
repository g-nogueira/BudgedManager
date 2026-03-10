using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
namespace MonthlyBudget.IdentityHousehold.Domain.Entities;
public class Household
{
    private readonly List<Member> _members = new();
    public Guid HouseholdId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<Member> Members => _members.AsReadOnly();
    private Household() { }
    public static Household Create(string name, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Household name required.", nameof(name));
        var household = new Household { HouseholdId = Guid.NewGuid(), Name = name.Trim(), CreatedAt = DateTime.UtcNow };
        household._members.Add(Member.Create(ownerId, MemberRole.OWNER));
        return household;
    }
    public void AddMember(Guid userId, MemberRole role)
    {
        // INV-H1: Max 2 members
        if (_members.Count >= 2) throw new HouseholdFullException();
        // INV-H2: Only one OWNER
        if (role == MemberRole.OWNER && _members.Any(m => m.Role == MemberRole.OWNER))
            throw new DuplicateOwnerException();
        _members.Add(Member.Create(userId, role));
    }

    /// <summary>
    /// INV-H4: Enforces that only one pending invitation may exist per household at a time.
    /// The caller resolves whether a pending invitation exists (requires a repo query)
    /// and passes the result so the invariant is declared and enforced in the domain.
    /// </summary>
    public void GuardPendingInvitation(bool hasPendingInvitation)
    {
        if (hasPendingInvitation)
            throw new PendingInvitationExistsException(HouseholdId);
    }
}
