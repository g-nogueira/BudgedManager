using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonthlyBudget.ForecastEngine.Domain.Entities;
namespace MonthlyBudget.Infrastructure.Database.Configurations;
public class DailyEntryConfiguration : IEntityTypeConfiguration<DailyEntry>
{
    public void Configure(EntityTypeBuilder<DailyEntry> builder)
    {
        builder.ToTable("daily_entries", "forecast");
        builder.HasKey(d => d.EntryId);
        builder.Property(d => d.EntryId).HasColumnName("entry_id").ValueGeneratedNever();
        builder.Property(d => d.ForecastId).HasColumnName("forecast_id").IsRequired();
        builder.Property(d => d.DayNumber).HasColumnName("day_number").IsRequired();
        builder.Property(d => d.RemainingBalance).HasColumnName("remaining_balance").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(d => d.DailyExpenseTotal).HasColumnName("daily_expense_total").HasColumnType("decimal(12,2)").IsRequired();
        builder.HasIndex(d => new { d.ForecastId, d.DayNumber }).IsUnique();
        builder.HasMany(d => d.ExpenseBreakdown)
               .WithOne().HasForeignKey(i => i.EntryId);
        builder.Navigation(d => d.ExpenseBreakdown).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
public class DailyExpenseItemConfiguration : IEntityTypeConfiguration<DailyExpenseItem>
{
    public void Configure(EntityTypeBuilder<DailyExpenseItem> builder)
    {
        builder.ToTable("daily_expense_items", "forecast");
        builder.HasKey(i => i.ItemId);
        builder.Property(i => i.ItemId).HasColumnName("item_id").ValueGeneratedNever();
        builder.Property(i => i.EntryId).HasColumnName("entry_id").IsRequired();
        builder.Property(i => i.ExpenseSnapshotId).HasColumnName("expense_snapshot_id").IsRequired();
        builder.Property(i => i.ExpenseName).HasColumnName("expense_name").HasMaxLength(100).IsRequired();
        builder.Property(i => i.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
    }
}
public class ExpenseSnapshotConfiguration : IEntityTypeConfiguration<ExpenseSnapshot>
{
    public void Configure(EntityTypeBuilder<ExpenseSnapshot> builder)
    {
        builder.ToTable("expense_snapshots", "forecast");
        builder.HasKey(s => s.SnapshotId);
        builder.Property(s => s.SnapshotId).HasColumnName("snapshot_id").ValueGeneratedNever();
        builder.Property(s => s.ForecastId).HasColumnName("forecast_id").IsRequired();
        builder.Property(s => s.OriginalExpenseId).HasColumnName("original_expense_id").IsRequired();
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(s => s.Category).HasColumnName("category").HasConversion<string>().IsRequired();
        builder.Property(s => s.DayOfMonth).HasColumnName("day_of_month");
        builder.Property(s => s.IsSpread).HasColumnName("is_spread").IsRequired();
        builder.Property(s => s.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(s => s.IsExcluded).HasColumnName("is_excluded").IsRequired();
    }
}
