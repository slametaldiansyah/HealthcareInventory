namespace HealthcareScheduling.Application.DTOs;

public record AvailabilitySlotDto(DateTimeOffset Start, DateTimeOffset End);
