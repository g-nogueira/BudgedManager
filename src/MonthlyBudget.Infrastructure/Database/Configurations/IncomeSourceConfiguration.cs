using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonthlyBudget.BudgetManagement.Domain.Entities;
namespace MonthlyBudget.Infrastructure.Database.Configurations;
public class IncomeSourceConfiguration : IEntityTypeConfiguration<IncomeSource>
{
    public void Configure(EntityTypeBuilder<IncomeSource> builder)
    {
        builder.ToTable("income_sources", "budget");
        builder.HasKey(i => i.IncomeId);
        builder.Property(i => i.IncomeId).HasColumnName("income_id");
        builder.Property<Guid>("BudgetId").HasColumnName("budget_id");
        builder.Property(i => i.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(i => i.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
