using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Events;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using Xunit;
namespace MonthlyBudget.IdentityHousehold.Tests.Domain;
public class HouseholdTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsSingleOwnerMember()
    {
        var ownerId = Guid.NewGuid();
        var hh = Household.Create("Smith Family", ownerId);
        Assert.Equal("Smith Family", hh.Name);
        Assert.Single(hh.Members);
        Assert.Equal(MemberRole.OWNER, hh.Members[0].Role);
        Assert.Equal(ownerId, hh.Members[0].UserId);
    }
    [Fact]
    public void AddMember_SecondMember_AddsPartner()
    {
        var hh = Household.Create("Smith Family", Guid.NewGuid());
        var partnerId = Guid.NewGuid();
        hh.AddMember(partnerId, MemberRole.PARTNER);
        Assert.Equal(2, hh.Members.Count);
        Assert.Contains(hh.Members, m => m.Role == MemberRole.PARTNER && m.UserId == partnerId);
    }
    [Fact]
    public void AddMember_ThirdMember_ThrowsHouseholdFull_INV_H1()
    {
        var hh = Household.Create("Smith Family", Guid.NewGuid());
        hh.AddMember(Guid.NewGuid(), MemberRole.PARTNER);
        Assert.Throws<HouseholdFullException>(() => hh.AddMember(Guid.NewGuid(), MemberRole.PARTNER));
    }
    [Fact]
    public void AddMember_SecondOwner_ThrowsDuplicateOwner_INV_H2()
    {
        var hh = Household.Create("Smith Family", Guid.NewGuid());
        Assert.Throws<DuplicateOwnerException>(() => hh.AddMember(Guid.NewGuid(), MemberRole.OWNER));
    }
    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Household.Create("", Guid.NewGuid()));
    }
}
public class UserTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsUser()
    {
        var user = User.Create("test@example.com", "Alice", "hash123");
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("Alice", user.DisplayName);
    }
    [Fact]
    public void Create_EmailNormalized_ToLowercase()
    {
        var user = User.Create("TEST@EXAMPLE.COM", "Alice", "hash");
        Assert.Equal("test@example.com", user.Email);
    }
    [Fact]
    public void Create_EmptyEmail_Throws()
    {
        Assert.Throws<ArgumentException>(() => User.Create("", "Alice", "hash"));
    }
}
public class InvitationTests
{
    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    [Fact]
    public void Create_ValidInputs_StatusIsPending()
    {
        var inv = Invitation.Create(Guid.NewGuid(), "partner@example.com");
        Assert.Equal(InvitationStatus.PENDING, inv.Status);
        Assert.False(string.IsNullOrEmpty(inv.Token));
    }
    [Fact]
    public void Accept_PendingInvitation_StatusBecomesAccepted()
    {
        var inv = Invitation.Create(Guid.NewGuid(), "partner@example.com");
        inv.Accept();
        Assert.Equal(InvitationStatus.ACCEPTED, inv.Status);
    }
    [Fact]
    public void Accept_AlreadyAccepted_ThrowsInvalidOperation()
    {
        var inv = Invitation.Create(Guid.NewGuid(), "partner@example.com");
        inv.Accept();
        Assert.Throws<InvalidOperationException>(() => inv.Accept());
    }

    [Fact]
    public void Accept_ExpiredInvitation_ThrowsInvitationExpiredException_INV_H5()
    {
        var inv = Invitation.Create(Guid.NewGuid(), "partner@example.com");
        var futureClock = new FixedTimeProvider(DateTimeOffset.UtcNow.AddDays(8));

        Assert.Throws<InvitationExpiredException>(() => inv.Accept(futureClock));
    }

    [Fact]
    public void Expire_SetsStatusToExpired()
    {
        var inv = Invitation.Create(Guid.NewGuid(), "partner@example.com");
        inv.Expire();
        Assert.Equal(InvitationStatus.EXPIRED, inv.Status);
    }
}

public class HouseholdGuardPendingInvitationTests
{
    [Fact]
    public void GuardPendingInvitation_NoPending_DoesNotThrow_INV_H4()
    {
        var hh = Household.Create("My Home", Guid.NewGuid());
        // hasPendingInvitation = false → no exception
        var ex = Record.Exception(() => hh.GuardPendingInvitation(false));
        Assert.Null(ex);
    }

    [Fact]
    public void GuardPendingInvitation_HasPending_ThrowsPendingInvitationExists_INV_H4()
    {
        var hh = Household.Create("My Home", Guid.NewGuid());
        Assert.Throws<PendingInvitationExistsException>(() => hh.GuardPendingInvitation(true));
    }
}

public class HouseholdDomainEventsTests
{
    [Fact]
    public void HouseholdCreated_SetsProperties()
    {
        var householdId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var evt = new HouseholdCreated(householdId, ownerId);
        Assert.Equal(householdId, evt.HouseholdId);
        Assert.Equal(ownerId, evt.OwnerId);
        Assert.NotEqual(Guid.Empty, evt.EventId);
    }

    [Fact]
    public void MemberInvited_SetsProperties()
    {
        var householdId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();
        const string email = "partner@example.com";
        var evt = new MemberInvited(householdId, invitationId, email);
        Assert.Equal(householdId, evt.HouseholdId);
        Assert.Equal(invitationId, evt.InvitationId);
        Assert.Equal(email, evt.InvitedEmail);
        Assert.NotEqual(Guid.Empty, evt.EventId);
    }

    [Fact]
    public void MemberJoined_SetsProperties()
    {
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string role = "PARTNER";
        var evt = new MemberJoined(householdId, userId, role);
        Assert.Equal(householdId, evt.HouseholdId);
        Assert.Equal(userId, evt.UserId);
        Assert.Equal(role, evt.Role);
        Assert.NotEqual(Guid.Empty, evt.EventId);
    }
}

public class IdentityExceptionsTests
{
    [Fact]
    public void InvitationNotFoundException_HasExpectedMessage()
    {
        var ex = new InvitationNotFoundException();
        Assert.Equal("Invitation not found.", ex.Message);
    }

    [Fact]
    public void UserAlreadyInHouseholdException_HasExpectedMessage()
    {
        var ex = new UserAlreadyInHouseholdException();
        Assert.Equal("User already belongs to a household.", ex.Message);
    }

    [Fact]
    public void InsufficientRoleException_HasExpectedMessage()
    {
        var ex = new InsufficientRoleException();
        Assert.Equal("Only the OWNER can perform this action.", ex.Message);
    }

    [Fact]
    public void PendingInvitationExistsException_ContainsHouseholdId()
    {
        var id = Guid.NewGuid();
        var ex = new PendingInvitationExistsException(id);
        Assert.Contains(id.ToString(), ex.Message);
    }
}

public class HouseholdAuthorizeInviteTests
{
    [Fact]
    public void AuthorizeInvite_WithOwner_DoesNotThrow()
    {
        var ownerId = Guid.NewGuid();
        var household = Household.Create("Test", ownerId);
        var ex = Record.Exception(() => household.AuthorizeInvite(ownerId));
        Assert.Null(ex);
    }

    [Fact]
    public void AuthorizeInvite_WithPartner_ThrowsInsufficientRoleException()
    {
        var ownerId = Guid.NewGuid();
        var partnerId = Guid.NewGuid();
        var household = Household.Create("Test", ownerId);
        household.AddMember(partnerId, MemberRole.PARTNER);
        Assert.Throws<InsufficientRoleException>(() => household.AuthorizeInvite(partnerId));
    }

    [Fact]
    public void AuthorizeInvite_WithUnknownUser_ThrowsInsufficientRoleException()
    {
        var household = Household.Create("Test", Guid.NewGuid());
        Assert.Throws<InsufficientRoleException>(() => household.AuthorizeInvite(Guid.NewGuid()));
    }
}
