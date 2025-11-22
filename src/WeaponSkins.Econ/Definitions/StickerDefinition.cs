namespace WeaponSkins.Econ;

public record StickerDefinition
{
    public required string Name { get; set; }
    public required int Index { get; set; }
    public required Dictionary<string, string> LocalizedNames { get; set; }
    public required RarityDefinition Rarity { get; set; }
}