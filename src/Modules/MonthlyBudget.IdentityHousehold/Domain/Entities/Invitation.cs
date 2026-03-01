namespace MonthlyBudget.IdentityHousehold.Domain.Entities;
public enum InvitationStatus { PENDING, ACCEPTED, EXPIRED }
public class Invitation
{
    public Guid InvitationId { get; private set; }
    public Guid HouseholdId { get; private set; }
    public string InvitedEmail { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public InvitationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    private Invitation() { }
    public static Invitation Create(Guid householdId, string email)
    {
        return new Invitation
        {
            InvitationId = Guid.NewGuid(), HouseholdId = householdId,
            InvitedEmail = email.Trim().ToLowerInvariant(),
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            Status = InvitationStatus.PENDING,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }
    public void Accept()
    {
        if (Status != InvitationStatus.PENDING) throw new InvalidOperationException("Only PENDING invitations can be accepted.");
        if (DateTime.UtcNow > ExpiresAt) throw new Exceptions.InvitationExpiredException();
        Status = InvitationStatus.ACCEPTED;
    }
    public void Expire() { Status = InvitationStatus.EXPIRED; }
}
