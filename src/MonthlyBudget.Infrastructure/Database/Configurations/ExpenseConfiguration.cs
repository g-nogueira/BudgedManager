using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonthlyBudget.BudgetManagement.Domain.Entities;
namespace MonthlyBudget.Infrastructure.Database.Configurations;
public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses", "budget");
        builder.HasKey(e => e.ExpenseId);
        builder.Property(e => e.ExpenseId).HasColumnName("expense_id").ValueGeneratedNever();
        builder.Property<Guid>("BudgetId").HasColumnName("budget_id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Category).HasColumnName("category").HasConversion<string>().IsRequired();
        builder.Property(e => e.DayOfMonth).HasColumnName("day_of_month");
        builder.Property(e => e.IsSpread).HasColumnName("is_spread").IsRequired();
        builder.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.IsExcluded).HasColumnName("is_excluded").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
