namespace DistrictPortal.Api.Data.Entities;

/// <summary>A Local Education Agency (district or charter school network).</summary>
public sealed class Lea
{
    public required string Id { get; init; }

    public required string Name { get; set; }

    public required string OrgLabel { get; set; }

    public required string DisplayName { get; set; }

    public List<CollectionDefinition> Collections { get; init; } = [];
}
