using SwiftlyS2.Shared.Natives;

namespace WeaponPaints;

public struct CGCClientSharedObjectCache : INativeHandle
{
  public nint Address { get; set; }
  public bool IsValid => Address != 0;

  private NativeService NativeService { get; init; }

  public CGCClientSharedObjectCache(nint address, NativeService nativeService)
  {
    Address = address;
    NativeService = nativeService;
  }

  public void AddObject(CEconItem item)
  {
    unsafe {
      NativeService.SOCache_AddObject.Call(Address, item.Address);
    }
  }

  public void RemoveObject(CEconItem item)
  {
    unsafe {
      NativeService.SOCache_RemoveObject.Call(Address, item.Address);
    } 
  }

  public SOID_t Owner => Address.AsRef<SOID_t>(NativeService.CGCClientSharedObjectCache_m_OwnerOffset);
}