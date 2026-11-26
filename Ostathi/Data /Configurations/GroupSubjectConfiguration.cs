using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ostathi;

public class GroupSubjectConfiguration : IEntityTypeConfiguration<GroupSubject>
{
    public void Configure(EntityTypeBuilder<GroupSubject> builder)
    {
        // Map entity to the GroupSubjects table in SQLite.
        builder.ToTable("GroupSubjects");

        // Configure primary key with auto-increment generation.
        builder.HasKey(gs => gs.Id);

        builder.Property(gs => gs.Id)
               .ValueGeneratedOnAdd();

        // Configure required Group foreign key.
        builder.Property(gs => gs.GroupId)
               .IsRequired();

        // Configure required Subject foreign key.
        builder.Property(gs => gs.SubjectId)
               .IsRequired();

        // Configure status and optional notes.
        builder.Property(gs => gs.Status)
               .IsRequired()
               .HasMaxLength(50)
               .HasDefaultValue("Active");

        builder.Property(gs => gs.Notes)
               .HasMaxLength(500);

        // Configure the relationship between GroupSubject and Group.
        builder.HasOne(gs => gs.Group)
               .WithMany()
               .HasForeignKey(gs => gs.GroupId)
               .OnDelete(DeleteBehavior.Cascade);

        // Configure the relationship between GroupSubject and Subject.
        builder.HasOne(gs => gs.Subject)
               .WithMany()
               .HasForeignKey(gs => gs.SubjectId)
               .OnDelete(DeleteBehavior.Restrict);

        // Create indexes for faster lookups and prevent duplicate assignments.
        builder.HasIndex(gs => gs.GroupId)
               .HasDatabaseName("IX_GroupSubjects_GroupId");

        builder.HasIndex(gs => gs.SubjectId)
               .HasDatabaseName("IX_GroupSubjects_SubjectId");

        builder.HasIndex(gs => new { gs.GroupId, gs.SubjectId })
               .IsUnique()
               .HasDatabaseName("IX_GroupSubjects_GroupId_SubjectId");
    }
}
