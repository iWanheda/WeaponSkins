using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace WeaponPaints;

[StructLayout(LayoutKind.Explicit, Size = 72)]
public ref struct CEconItemStruct
{

  [FieldOffset(0x10)]
  public ulong ItemID;

  [FieldOffset(0x20)]
  public nint pCustomData;

  [FieldOffset(0x28)]
  public uint AccountID;

  [FieldOffset(0x2C)]
  public uint InventoryPosition; // Backpack slot

  [FieldOffset(0x30)]
  public ushort DefinitionIndex;

  [FieldOffset(0x34)]
  private ushort _packedBits;

  private const int ORIGIN_SHIFT = 0;
  private const ushort ORIGIN_MASK = (ushort)((1 << 5) - 1); // 0b1_1111

  private const int QUALITY_SHIFT = 5;
  private const ushort QUALITY_MASK = (ushort)((1 << 4) - 1); // 0b1111

  private const int LEVEL_SHIFT = 9;
  private const ushort LEVEL_MASK = (ushort)((1 << 2) - 1); // 0b11

  private const int RARITY_SHIFT = 11;
  private const ushort RARITY_MASK = (ushort)((1 << 4) - 1); // 0b1111

  private const int INUSE_SHIFT = 15;
  private const ushort INUSE_MASK = 0x0001; // 1 bit

  public ushort Origin
  {
    get => (ushort)((_packedBits >> ORIGIN_SHIFT) & ORIGIN_MASK);
    set
    {
      if (value > ORIGIN_MASK) throw new ArgumentOutOfRangeException(nameof(value), "Origin out of 5 bit range.");
      _packedBits = (ushort)((_packedBits & ~(ORIGIN_MASK << ORIGIN_SHIFT)) | ((value & ORIGIN_MASK) << ORIGIN_SHIFT));
    }
  }

  public ushort Quality
  {
    get => (ushort)((_packedBits >> QUALITY_SHIFT) & QUALITY_MASK);
    set
    {
      if (value > QUALITY_MASK) throw new ArgumentOutOfRangeException(nameof(value), "Quality out of 4bit range.");
      _packedBits = (ushort)((_packedBits & ~(QUALITY_MASK << QUALITY_SHIFT)) | ((value & QUALITY_MASK) << QUALITY_SHIFT));
    }
  }

  public ushort Level
  {
    get => (ushort)((_packedBits >> LEVEL_SHIFT) & LEVEL_MASK);
    set
    {
      if (value > LEVEL_MASK) throw new ArgumentOutOfRangeException(nameof(value), "Level out of 2 bit range.");
      _packedBits = (ushort)((_packedBits & ~(LEVEL_MASK << LEVEL_SHIFT)) | ((value & LEVEL_MASK) << LEVEL_SHIFT));
    }
  }

  public ushort Rarity
  {
    get => (ushort)((_packedBits >> RARITY_SHIFT) & RARITY_MASK);
    set
    {
      if (value > RARITY_MASK) throw new ArgumentOutOfRangeException(nameof(value), "Rarity out of 4 bit range.");
      _packedBits = (ushort)((_packedBits & ~(RARITY_MASK << RARITY_SHIFT)) | ((value & RARITY_MASK) << RARITY_SHIFT));
    }
  }

  public bool InUse
  {
    get => ((_packedBits >> INUSE_SHIFT) & INUSE_MASK) != 0;
    set
    {
      if (value)
        _packedBits = (ushort)(_packedBits | (INUSE_MASK << INUSE_SHIFT));
      else
        _packedBits = (ushort)(_packedBits & ~(INUSE_MASK << INUSE_SHIFT));
    }
  }


}

public class CEconItem : INativeHandle
{
  public nint Address { get; set; }
  public bool IsValid => Address != 0;

  public CEconItem(nint address)
  {
    Address = address;
  }

  private ref CEconItemStruct Struct => ref Address.AsRef<CEconItemStruct>();

  public ulong ItemID { get => Struct.ItemID; set => Struct.ItemID = value; }
  public nint pCustomData { get => Struct.pCustomData; set => Struct.pCustomData = value; }
  public uint AccountID { get => Struct.AccountID; set => Struct.AccountID = value; }
  public uint InventoryPosition { get => Struct.InventoryPosition; set => Struct.InventoryPosition = value; }
  public ushort DefinitionIndex { get => Struct.DefinitionIndex; set => Struct.DefinitionIndex = value; }
  public ushort Origin { get => Struct.Origin; set => Struct.Origin = value; }
  public ushort Quality { get => Struct.Quality; set => Struct.Quality = value; }
  public ushort Level { get => Struct.Level; set => Struct.Level = value; }
  public ushort Rarity { get => Struct.Rarity; set => Struct.Rarity = value; }
  public bool InUse { get => Struct.InUse; set => Struct.InUse = value; }


  public void ConfigureAttributes(Action<CustomAttributeData> configure)
  {
    if (pCustomData == 0)
    {
      pCustomData = CustomAttributeData.Create().Address;
    }
    var customData = new CustomAttributeData(pCustomData);
    configure(customData);
    pCustomData = customData.Address; // handle realloc
  }

  public void Apply(WeaponSkinData data)
  {
    DefinitionIndex = data.DefinitionIndex;
    ConfigureAttributes(customData =>
    {
      customData.SetPaintkit(data.Paintkit);
      customData.SetPaintkitSeed(data.PaintkitSeed);
      customData.SetPaintkitWear(data.PaintkitWear);
    });
  }
}