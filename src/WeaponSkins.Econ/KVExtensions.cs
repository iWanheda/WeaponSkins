using System.Globalization;

using ValveKeyValue;

namespace WeaponSkins.Econ;

public static class KVExtensions
{
    public static int EToInt32(this KVValue value)
    {
        return value.ToInt32(CultureInfo.InvariantCulture);
    }

    public static string EToString(this KVValue value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static bool HasSubKey(this KVObject obj, string key)
    {
        return obj.Children.Any(c => c.Name == key);
    }

    public static bool HasSubKeyWithValue(this KVObject obj, string key, string value)
    {
        return obj.Children.Any(c => c.Name == key && c.Value.EToString() == value);
    }
}