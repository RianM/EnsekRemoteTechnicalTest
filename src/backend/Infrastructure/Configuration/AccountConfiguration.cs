using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        
        builder.HasKey(a => a.AccountId);
        
        builder.Property(a => a.AccountId)
            .HasColumnName("AccountId")
            .IsRequired()
            .ValueGeneratedOnAdd();
            
        builder.Property(a => a.FirstName)
            .HasColumnName("FirstName")
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(a => a.LastName)
            .HasColumnName("LastName")
            .IsRequired()
            .HasMaxLength(100);
    }
}