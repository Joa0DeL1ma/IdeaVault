using IdeaVault.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdeaVault.Infra.Data.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Emoji).HasMaxLength(16).IsRequired();
        builder.Property(c => c.Color).HasMaxLength(7).IsRequired();
        builder.Property(c => c.TemplateSections); // text[] on PostgreSQL
        builder.Ignore(c => c.IsInbox);

        builder.HasMany(c => c.Ideas)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
