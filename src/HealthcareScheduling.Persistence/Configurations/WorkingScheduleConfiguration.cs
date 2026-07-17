using HealthcareScheduling.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareScheduling.Persistence.Configurations;

public class WorkingScheduleConfiguration : IEntityTypeConfiguration<WorkingSchedule>
{
    public void Configure(EntityTypeBuilder<WorkingSchedule> builder)
    {
        builder.ToTable("WorkingSchedules");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.StartTime)
            .IsRequired();

        builder.Property(w => w.EndTime)
            .IsRequired();

        builder.HasIndex(w => new { w.DoctorId, w.DayOfWeek })
            .HasDatabaseName("IX_WorkingSchedules_DoctorId_DayOfWeek");
    }
}
