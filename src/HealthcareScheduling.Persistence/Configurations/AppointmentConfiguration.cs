using HealthcareScheduling.Domain.Entities;
using HealthcareScheduling.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthcareScheduling.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.StartUtc)
            .IsRequired();

        builder.Property(a => a.EndUtc)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(AppointmentStatus.Active);

        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        builder.HasIndex(a => new { a.DoctorId, a.StartUtc })
            .HasDatabaseName("IX_Appointments_DoctorId_StartUtc");

        builder.HasIndex(a => new { a.DoctorId, a.StartUtc, a.EndUtc })
            .HasDatabaseName("IX_Appointments_DoctorId_Range");
    }
}
