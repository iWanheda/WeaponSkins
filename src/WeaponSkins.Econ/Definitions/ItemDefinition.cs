namespace WeaponSkins.Econ;

public record ItemDefinition
{
    public required string Name { get; set; }
    public required int Index { get; set; }
    public required Dictionary<string, string> LocalizedNames { get; set; }
}