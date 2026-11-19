using IdeaVault.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdeaVault.Infra.Data.Configurations;

internal sealed class IdeaSectionConfiguration : IEntityTypeConfiguration<IdeaSection>
{
    public void Configure(EntityTypeBuilder<IdeaSection> builder)
    {
        builder.ToTable("idea_sections");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Content).IsRequired();

        builder.HasIndex(s => new { s.IdeaId, s.SortOrder });
    }
}
