namespace WeaponSkins;

public static class Utilities
{
    public static bool IsKnifeDefinitionIndex(int def)
    {
      return def is 42 or 59 or (>= 500 and < 600);
    }
}