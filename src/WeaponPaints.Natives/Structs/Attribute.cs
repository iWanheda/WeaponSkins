using System.Runtime.InteropServices;

namespace WeaponPaints;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Attribute
{
  [FieldOffset(0)]
  public ushort AttributeDefinitionIndex;

  [FieldOffset(8)]
  public float FloatData;

  [FieldOffset(8)]
  public int IntData;
}