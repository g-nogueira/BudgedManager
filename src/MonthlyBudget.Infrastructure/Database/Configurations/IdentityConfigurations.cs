using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
namespace MonthlyBudget.Infrastructure.Database.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("app_users", "identity");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).HasColumnName("user_id");
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(u => u.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
public class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        builder.ToTable("households", "identity");
        builder.HasKey(h => h.HouseholdId);
        builder.Property(h => h.HouseholdId).HasColumnName("household_id");
        builder.Property(h => h.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(h => h.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.OwnsMany(h => h.Members, m =>
        {
            m.ToTable("household_members", "identity");
            m.WithOwner().HasForeignKey("HouseholdId");
            m.Property<Guid>("MemberId").HasColumnName("member_id").ValueGeneratedOnAdd();
            m.HasKey("MemberId");
            m.Property(mb => mb.UserId).HasColumnName("user_id").IsRequired();
            m.Property(mb => mb.Role).HasColumnName("role").HasConversion<string>().IsRequired();
            m.Property(mb => mb.JoinedAt).HasColumnName("joined_at").IsRequired();
        });
    }
}
public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations", "identity");
        builder.HasKey(i => i.InvitationId);
        builder.Property(i => i.InvitationId).HasColumnName("invitation_id");
        builder.Property(i => i.HouseholdId).HasColumnName("household_id").IsRequired();
        builder.Property(i => i.InvitedEmail).HasColumnName("invited_email").HasMaxLength(255).IsRequired();
        builder.Property(i => i.Token).HasColumnName("token").HasMaxLength(100).IsRequired();
        builder.Property(i => i.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(i => i.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.HasIndex(i => i.Token).IsUnique();
    }
}
