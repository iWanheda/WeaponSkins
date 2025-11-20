using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Memory;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace WeaponPaints;

public class NativeService
{

  private ISwiftlyCore Core { get; init; }
  private ILogger<NativeService> Logger { get; init; }

  public unsafe delegate nint CreateCEconItemDelegate();

  public unsafe delegate byte AddObjectDelegate(nint pSOCache, nint pSharedObject);
  public unsafe delegate byte RemoveObjectDelegate(nint pSOCache, nint pSharedObject);
  public unsafe delegate void SOUpdatedDelegate(nint pInventory, SOID_t* pSOID, nint pSharedObj, int eventType);
  public unsafe delegate void SODestroyedDelegate(nint pInventory, SOID_t* pSOID, nint pSharedObj, int eventType);
  public unsafe delegate nint SOCacheSubscribedDelegate(nint pInventory, SOID_t* pSOID, int eventType);
  public delegate nint GetEconItemByItemIDDelegate(nint pInventory, ulong itemid);

  public IUnmanagedFunction<CreateCEconItemDelegate> CreateCEconItem { get; private set; }
  public IUnmanagedFunction<AddObjectDelegate> SOCache_AddObject { get; private set; }
  public IUnmanagedFunction<RemoveObjectDelegate> SOCache_RemoveObject { get; private set; }
  public IUnmanagedFunction<SOUpdatedDelegate> CPlayerInventory_SOUpdated { get; private set; }
  public IUnmanagedFunction<SODestroyedDelegate> CPlayerInventory_SODestroyed { get; private set; }
  public IUnmanagedFunction<SOCacheSubscribedDelegate> CPlayerInventory_SOCacheSubscribed { get; private set; }
  public IUnmanagedFunction<GetEconItemByItemIDDelegate> GetEconItemByItemID { get; private set; }

  public int CCSPlayerInventory_LoadoutsOffset { get; private set; }
  public int CCSInventoryManager_m_DefaultLoadoutsOffset { get; private set; }
  public int CGCClientSharedObjectCache_m_OwnerOffset { get; private set; }
  public int CCSPlayerInventory_m_pSOCacheOffset { get; private set; }
  public CCSInventoryManager CCSInventoryManager { get; private set; }


  public event Action<CCSPlayerInventory, SOID_t> OnSOCacheSubscribed;


  public NativeService(ISwiftlyCore core, ILogger<NativeService> logger )
  {
    Core = core;
    Logger = logger;
    Initialize();
  }

  public void Initialize()
  {

    var soCacheVtable = Core.Memory.GetVTableAddress("server", "GCSDK::CGCClientSharedObjectCache")!.Value;
    SOCache_AddObject = Core.Memory.GetUnmanagedFunctionByVTable<AddObjectDelegate>(
      soCacheVtable,
      Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::AddObject")
    );

    SOCache_RemoveObject = Core.Memory.GetUnmanagedFunctionByVTable<RemoveObjectDelegate>(
      soCacheVtable,
      Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::RemoveObject")
    );

    var playerInventoryVtable = Core.Memory.GetVTableAddress("server", "CCSPlayerInventory")!.Value;

    CPlayerInventory_SOUpdated = Core.Memory.GetUnmanagedFunctionByVTable<SOUpdatedDelegate>(
      playerInventoryVtable,
      Core.GameData.GetOffset("CPlayerInventory::SOUpdated")
    );

    CPlayerInventory_SODestroyed = Core.Memory.GetUnmanagedFunctionByVTable<SODestroyedDelegate>(
      playerInventoryVtable,
      Core.GameData.GetOffset("CPlayerInventory::SODestroyed")
    );

    CPlayerInventory_SOCacheSubscribed = Core.Memory.GetUnmanagedFunctionByVTable<SOCacheSubscribedDelegate>(
      playerInventoryVtable,
      Core.GameData.GetOffset("CPlayerInventory::SoCacheSubscribed")
    );

    CreateCEconItem = Core.Memory.GetUnmanagedFunctionByAddress<CreateCEconItemDelegate>(
      Core.GameData.GetSignature("CreateCEconItem")
    );

    GetEconItemByItemID = Core.Memory.GetUnmanagedFunctionByAddress<GetEconItemByItemIDDelegate>(
      Core.GameData.GetSignature("GetEconItemByItemID")
    );

    CCSPlayerInventory_LoadoutsOffset = Core.GameData.GetOffset("CCSPlayerInventory::m_Loadouts");
    CCSInventoryManager_m_DefaultLoadoutsOffset = Core.GameData.GetOffset("CCSInventoryManager::m_DefaultLoadouts");
    CCSPlayerInventory_m_pSOCacheOffset = Core.GameData.GetOffset("CCSPlayerInventory::m_pSOCache");
    CGCClientSharedObjectCache_m_OwnerOffset = Core.GameData.GetOffset("GCSDK::CGCClientSharedObjectCache::m_Owner");
    var xrefCCSInventoryManager = Core.GameData.GetSignature("CCSInventoryManager_xref");
    CCSInventoryManager = new CCSInventoryManager(Core.Memory.ResolveXrefAddress(xrefCCSInventoryManager)!, this);
    CPlayerInventory_SOCacheSubscribed.AddHook(next => {
      unsafe {
        return (pInventory, pSOID, eventType) => {
            try {
              var ret = next()(pInventory, pSOID, eventType);
              var inventory = new CCSPlayerInventory(pInventory, this);
              var a = inventory.Loadouts[Team.CT, loadout_slot_t.LOADOUT_SLOT_C4];
            OnSOCacheSubscribed?.Invoke(inventory, *pSOID);
              return ret;
            } catch (Exception e) {
              Logger.LogError(e, "Error in SOCacheSubscribed");
              return 0;
            }
        };
      }
    });
  }

  public CEconItem CreateCEconItemInstance()
  {
    return new CEconItem(CreateCEconItem.Call());
  }

}