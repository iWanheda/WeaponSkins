namespace WeaponSkins.Econ;

public record RarityDefinition
{
    public required string Name { get; set; }
    public required int Id { get; set; }
    public required ColorDefinition Color { get; set; }
}