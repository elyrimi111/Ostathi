using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ostathi;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
public void Configure(EntityTypeBuilder<Student> builder)
{
// Map the entity to the Students table.
builder.ToTable("Students");

    // Configure the primary key.
    builder.HasKey(s => s.Id);

    builder.Property(s => s.Id)
           .ValueGeneratedOnAdd();

    // Configure student name properties.
    builder.Property(s => s.FirstName)
           .IsRequired()
           .HasMaxLength(100);

    builder.Property(s => s.SecondName)
           .IsRequired()
           .HasMaxLength(100);

    builder.Property(s => s.ThirdName)
           .IsRequired()
           .HasMaxLength(100);

    builder.Property(s => s.LastName)
           .IsRequired()
           .HasMaxLength(100);

    // Configure the academic identifier.
    builder.Property(s => s.AcademicId)
           .IsRequired()
           .HasMaxLength(50);

    // Configure the academic year.
    builder.Property(s => s.AcademicYear)
           .IsRequired()
           .HasMaxLength(100);

    // Configure the relationship between Student and Major.
    builder.HasOne(s => s.Major)
           .WithMany()
           .HasForeignKey(s => s.MajorId)
           .OnDelete(DeleteBehavior.Restrict);

    // Configure the relationship between Student and Group.
builder.HasOne(s => s.Group)
       .WithMany()
       .HasForeignKey(s => s.GroupId)
       .OnDelete(DeleteBehavior.Restrict);

    // Create a unique index for the academic identifier.
    builder.HasIndex(s => s.AcademicId)
           .IsUnique()
           .HasDatabaseName("IX_Students_AcademicId");

    // Create an index for filtering students by Major.
    builder.HasIndex(s => s.MajorId)
           .HasDatabaseName("IX_Students_MajorId");

    // Create an index for filtering students by Group.
    builder.HasIndex(s => s.GroupId)
           .HasDatabaseName("IX_Students_GroupId");

    // Create a composite index for filtering by Major and Academic Year.
    builder.HasIndex(s => new { s.MajorId, s.AcademicYear })
           .HasDatabaseName("IX_Students_Major_AcademicYear");
}

}