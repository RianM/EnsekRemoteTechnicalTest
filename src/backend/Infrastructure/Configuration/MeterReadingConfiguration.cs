using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> builder)
    {
        builder.ToTable("MeterReadings", b=>b.HasCheckConstraint("CK_MeterReadings_MeterReadValue", "\"MeterReadValue\" >= 0 AND \"MeterReadValue\" <= 99999"));

        builder.HasKey(mr => new { mr.AccountId, mr.MeterReadingDateTime });

        builder.Property(mr => mr.AccountId)
            .HasColumnName("AccountId")
            .IsRequired();
            
        builder.Property(mr => mr.MeterReadingDateTime)
            .HasColumnName("MeterReadingDateTime")
            .IsRequired();
            
        builder.Property(mr => mr.MeterReadValue)
            .HasColumnName("MeterReadValue")
            .IsRequired();
            
        builder.HasOne(mr => mr.Account)
            .WithMany(a => a.MeterReadings)
            .HasForeignKey(mr => mr.AccountId)
            .HasPrincipalKey(a => a.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}