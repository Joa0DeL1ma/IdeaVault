namespace IdeaVault.Core.Dtos;

/// <summary>Full backup of the vault.</summary>
public sealed record ExportDto(
    int SchemaVersion,
    DateTime ExportedAt,
    IReadOnlyList<CategoryDto> Categories,
    IReadOnlyList<IdeaDetailDto> Ideas);
