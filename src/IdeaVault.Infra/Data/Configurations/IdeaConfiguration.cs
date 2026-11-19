using IdeaVault.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdeaVault.Infra.Data.Configurations;

internal sealed class IdeaConfiguration : IEntityTypeConfiguration<Idea>
{
    public void Configure(EntityTypeBuilder<Idea> builder)
    {
        builder.ToTable("ideas", t => t.HasCheckConstraint(
            "ck_ideas_excitement_range",
            $"\"Excitement\" BETWEEN {Idea.MinExcitement} AND {Idea.MaxExcitement}"));

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Summary).HasMaxLength(2000);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Tags); // text[] on PostgreSQL

        builder.HasMany(i => i.Sections)
            .WithOne()
            .HasForeignKey(s => s.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.CategoryId);
        builder.HasIndex(i => i.UpdatedAt);
    }
}
