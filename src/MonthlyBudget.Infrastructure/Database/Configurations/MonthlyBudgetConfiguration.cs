using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BudgetEntity = MonthlyBudget.BudgetManagement.Domain.Entities.MonthlyBudget;

namespace MonthlyBudget.Infrastructure.Database.Configurations;

public class MonthlyBudgetConfiguration : IEntityTypeConfiguration<BudgetEntity>
{
    public void Configure(EntityTypeBuilder<BudgetEntity> builder)
    {
        builder.ToTable("monthly_budgets", "budget");
        builder.HasKey(b => b.BudgetId);
        builder.Property(b => b.BudgetId).HasColumnName("budget_id");
        builder.Property(b => b.HouseholdId).HasColumnName("household_id").IsRequired();
        builder.Property(b => b.YearMonth).HasColumnName("year_month").HasMaxLength(7).IsRequired();
        builder.Property(b => b.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.HasIndex(b => new { b.HouseholdId, b.YearMonth }).IsUnique();

        // Use navigation-property overload so EF maps to the single BudgetId FK, no shadow duplicates
        builder.HasMany(b => b.IncomeSources)
               .WithOne().HasForeignKey("BudgetId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(b => b.Expenses)
               .WithOne().HasForeignKey("BudgetId").OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.IncomeSources).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(b => b.Expenses).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
