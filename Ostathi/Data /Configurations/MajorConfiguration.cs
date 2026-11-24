using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ostathi;

public class MajorConfiguration : IEntityTypeConfiguration<Major>
{
    public void Configure(EntityTypeBuilder<Major> builder)
    {
        // Map entity to the Majors table in SQLite
        builder.ToTable("Majors");

        // Configure primary key with auto-increment generation
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
               .ValueGeneratedOnAdd();

        // Configure required major name with length limit
        builder.Property(m => m.Name)
               .IsRequired()
               .HasMaxLength(150);

        // Configure major duration in years with default value of 4
        builder.Property(m => m.DurationYears)
               .IsRequired()
               .HasDefaultValue(4);

        // Add unique index to prevent duplicate major names
        builder.HasIndex(m => m.Name)
               .IsUnique()
               .HasDatabaseName("IX_Majors_Name");
    }
}
