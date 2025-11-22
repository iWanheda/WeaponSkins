namespace WeaponSkins.Econ;

public record PaintkitDefinition
{
    public required string Name { get; set; }
    public required int Index { get; set; }
    public required bool UseLegacyModel { get; set; }
    public required string DescriptionTag { get; set; }
    public required Dictionary<string, string> LocalizedNames { get; set; }
    public required RarityDefinition Rarity { get; set; }
}