using System.Runtime.CompilerServices;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;

namespace WeaponPaints;

public class CustomAttributeData : INativeHandle
{
  [SwiftlyInject]
  private static ISwiftlyCore Core { get; } = null!;

  public nint Address { get; set; }
  public bool IsValid => Address != 0;

  public CustomAttributeData(nint address)
  {
    Address = address;
  }

  private ref byte _flags => ref Address.AsRef<byte>(0);
  public ref byte Count => ref Address.AsRef<byte>(2);

  public static CustomAttributeData Create()
  {
    unsafe {
      var addr = Core.Memory.Alloc(4);
      var data = new CustomAttributeData(addr);
      data._flags = 0x3F;
      data.Count = 0;
      return data;
    }
  }

  public void AddAttribute(Attribute attribute)
  {
    Address = Core.Memory.Resize(Address, (ulong)(4 + Count * 16));
    this[Count] = attribute;
    Count++;
  }

  public void UpdateAttribute(Attribute attribute)
  {
    for (int i = 0; i < Count; i++)
    {
      if (this[i].AttributeDefinitionIndex == attribute.AttributeDefinitionIndex)
      {
        this[i] = attribute;
        return;
      }
    }
    AddAttribute(attribute);
  }

  public ref Attribute this[int index] => ref Address.AsRef<Attribute>(4 + index * Unsafe.SizeOf<Attribute>());

  public void SetPaintkit(int paintkit)
  {
    UpdateAttribute(new Attribute { AttributeDefinitionIndex = 6, FloatData = Convert.ToSingle(paintkit) });
  }

  public void SetPaintkitSeed(int seed)
  {
    UpdateAttribute(new Attribute { AttributeDefinitionIndex = 7, IntData = seed });
  }

  public void SetPaintkitWear(float wear)
  {
    UpdateAttribute(new Attribute { AttributeDefinitionIndex = 8, FloatData = wear });
  }

}