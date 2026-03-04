using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonthlyBudget.ForecastEngine.Domain.Entities;

namespace MonthlyBudget.Infrastructure.Database.Configurations;

public class ForecastVersionConfiguration : IEntityTypeConfiguration<ForecastVersion>
{
    public void Configure(EntityTypeBuilder<ForecastVersion> builder)
    {
        builder.ToTable("forecast_versions", "forecast");
        builder.HasKey(f => f.ForecastId);
        builder.Property(f => f.ForecastId).HasColumnName("forecast_id").ValueGeneratedNever();
        builder.Property(f => f.BudgetId).HasColumnName("budget_id").IsRequired();
        builder.Property(f => f.HouseholdId).HasColumnName("household_id").IsRequired();
        builder.Property(f => f.VersionLabel).HasColumnName("version_label").HasMaxLength(50).IsRequired();
        builder.Property(f => f.ForecastDate).HasColumnName("forecast_date").IsRequired();
        builder.Property(f => f.StartDay).HasColumnName("start_day").IsRequired();
        builder.Property(f => f.StartBalance).HasColumnName("start_balance").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(f => f.ActualBalance).HasColumnName("actual_balance").HasColumnType("decimal(12,2)");
        builder.Property(f => f.ForecastType).HasColumnName("forecast_type").HasConversion<string>().IsRequired();
        builder.Property(f => f.ParentForecastId).HasColumnName("parent_forecast_id");
        builder.Property(f => f.IsSnapshot).HasColumnName("is_snapshot").IsRequired();
        builder.Property(f => f.CreatedAt).HasColumnName("created_at").IsRequired();

        // Use navigation overloads to avoid shadow FK duplicates
        builder.HasMany(f => f.DailyEntries)
               .WithOne().HasForeignKey(e => e.ForecastId);
        builder.HasMany(f => f.ExpenseSnapshots)
               .WithOne().HasForeignKey(s => s.ForecastId);

        builder.Navigation(f => f.DailyEntries).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(f => f.ExpenseSnapshots).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
