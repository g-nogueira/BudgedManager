using MonthlyBudget.IdentityHousehold.Domain.Entities;
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
    public void Expire_SetsStatusToExpired()
    {
        var inv = Invitation.Create(Guid.NewGuid(), "partner@example.com");
        inv.Expire();
        Assert.Equal(InvitationStatus.EXPIRED, inv.Status);
    }
}
