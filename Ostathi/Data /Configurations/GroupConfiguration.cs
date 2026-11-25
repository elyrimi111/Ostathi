using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ostathi;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        // Map entity to the Groups table in SQLite.
        builder.ToTable("Groups");

        // Configure primary key with auto-increment generation.
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
               .ValueGeneratedOnAdd();

        // Configure required group name with length limit.
        builder.Property(g => g.Name)
               .IsRequired()
               .HasMaxLength(150);

        // Configure optional description and academic year.
        builder.Property(g => g.Description)
               .HasMaxLength(500);

        builder.Property(g => g.AcademicYear)
               .HasMaxLength(100);

        // Configure required Major foreign key.
        builder.Property(g => g.MajorId)
               .IsRequired();

        // Configure nullable representative student foreign key.
        builder.Property(g => g.RepresentativeStudentId)
               .IsRequired(false);

        // Configure the relationship between Group and Major.
        builder.HasOne(g => g.Major)
               .WithMany()
               .HasForeignKey(g => g.MajorId)
               .OnDelete(DeleteBehavior.Restrict);

        // Configure the relationship between Group and Representative Student.
        builder.HasOne(g => g.RepresentativeStudent)
               .WithMany()
               .HasForeignKey(g => g.RepresentativeStudentId)
               .OnDelete(DeleteBehavior.SetNull);

        // Create an index for MajorId.
        builder.HasIndex(g => g.MajorId)
               .HasDatabaseName("IX_Groups_MajorId");

        // Create an index for RepresentativeStudentId.
        builder.HasIndex(g => g.RepresentativeStudentId)
               .HasDatabaseName("IX_Groups_RepresentativeStudentId");

        // Create a composite index for MajorId, AcademicYear, and Name.
        builder.HasIndex(g => new
        {
            g.MajorId,
            g.AcademicYear,
            g.Name
        })
        .HasDatabaseName("IX_Groups_Major_Year_Name");
    }
}