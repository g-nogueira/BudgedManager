namespace MonthlyBudget.IdentityHousehold.Domain.Entities;
public enum MemberRole { OWNER, PARTNER }
public class Member
{
    public Guid UserId { get; private set; }
    public MemberRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    private Member() { }
    public static Member Create(Guid userId, MemberRole role) => new() { UserId = userId, Role = role, JoinedAt = DateTime.UtcNow };
}
