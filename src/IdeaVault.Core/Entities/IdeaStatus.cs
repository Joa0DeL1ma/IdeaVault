namespace IdeaVault.Core.Entities;

/// <summary>Lifecycle of an idea, from a raw thought to something being built (or shelved).</summary>
public enum IdeaStatus
{
    Seed,
    Growing,
    Executing,
    Archived,
}
