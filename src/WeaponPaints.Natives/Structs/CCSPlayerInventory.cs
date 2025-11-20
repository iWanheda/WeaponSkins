using System.Runtime.CompilerServices;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

namespace WeaponPaints;

public class CCSPlayerInventory : INativeHandle
{
  public nint Address { get; set; }
  public bool IsValid => Address != 0;

  public ulong SteamID => SOCache.Owner.SteamID;

  private NativeService NativeService { get; init; }

  public CCSPlayerInventory(nint address, NativeService nativeService)
  {
    Address = address;
    NativeService = nativeService;
  }

  public CGCClientSharedObjectCache SOCache => new CGCClientSharedObjectCache(Address.Read<nint>(NativeService.CCSPlayerInventory_m_pSOCacheOffset), NativeService);

  public void SODestroyed(ulong steamid, CEconItem item)
  {
    var soid = new SOID_t(steamid);
    unsafe
    {
      NativeService.CPlayerInventory_SODestroyed.Call(Address, &soid, item.Address, 4 /* eSOCacheEvent_Incremental */);
    }
  }

  public void SOUpdated(ulong steamid, CEconItem item)
  {
    var soid = new SOID_t(steamid);
    unsafe
    {
      NativeService.CPlayerInventory_SOUpdated.Call(Address, &soid, item.Address, 4 /* eSOCacheEvent_Incremental */);
    }
  }

  public CEconItem? GetEconItemByItemID(ulong itemid)
  {
    unsafe {
      var ptr = NativeService.GetEconItemByItemID.Call(Address, itemid);
      if (ptr == 0)
      {
        return null;
      }
      return new CEconItem(ptr);
    }
  }

  public ref CCSPlayerInventory_Loadouts Loadouts => ref Address.AsRef<CCSPlayerInventory_Loadouts>(NativeService.CCSPlayerInventory_LoadoutsOffset);

  private bool TryGetLoadoutItem(Team team, ushort definitionIndex, ref LoadoutItem loadoutItem)
  {
    foreach (var slot in Enum.GetValues<loadout_slot_t>())
    {
      if (Loadouts[team, slot].DefinitionIndex == definitionIndex)
      {
        loadoutItem = ref Loadouts[team, slot];
        return true;
      }
    }
    // itemid ==0, defindex == 65535, meaning the loadout slot is default weapon and default skin
    // so we search in default loadouts.
    foreach (var (slot, itemView) in NativeService.CCSInventoryManager.GetDefaultLoadouts(team))
    {
      if (itemView.ItemDefinitionIndex == definitionIndex)
      {
        loadoutItem = ref Loadouts[team, slot];
        return true;
      }
    }
    // absent in loadout
    return false;
  }

  private bool IsValidItemID(ulong itemID)
  {
    // 0xF00000000000000: default skin so no item id, but loadout item changed
    return itemID != 0 && itemID < 0xF000000000000000;
  }

  public bool TryGetItemID(Team team, ushort definitionIndex, out ulong itemID)
  {
    itemID = 0;
    LoadoutItem loadoutItem = default;
    if (!TryGetLoadoutItem(team, definitionIndex, ref loadoutItem))
    {
      return false;
    }
    itemID = loadoutItem.ItemId;
    if (!IsValidItemID(itemID))
    {
      itemID = 0;
      return false;
    }
    return true;
  }

  public void UpdateLoadoutItem(Team team, ushort definitionIndex, ulong itemID)
  {
    LoadoutItem loadoutItem = default;
    if (!TryGetLoadoutItem(team, definitionIndex, ref loadoutItem))
    {
      // do nothing
      return;
    }
    loadoutItem.ItemId = itemID;
    loadoutItem.DefinitionIndex = definitionIndex;
  }

  public void UpdateWeaponSkin(WeaponSkinData skinData)
  {
    unsafe {
      var item = NativeService.CreateCEconItemInstance();
      // Already has a skin
      if (TryGetItemID(skinData.Team, skinData.DefinitionIndex, out var itemID))
      {
        var oldItem = GetEconItemByItemID(itemID); // this should never be null, since item id is already in loadouts
        if (oldItem == null)
        {
          throw new Exception($"GetEconItemByItemID returned null for item id {itemID}");
        }
        item.AccountID = oldItem.AccountID;
        item.ItemID = oldItem.ItemID;
        SOCache.RemoveObject(oldItem);
        SODestroyed(SteamID, oldItem);
      } else {
        item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
        // not sure if it will work
        item.ItemID = (ulong)DateTime.UtcNow.Ticks;

        UpdateLoadoutItem(skinData.Team, skinData.DefinitionIndex, item.ItemID);
      }

      item.Apply(skinData);
      SOCache.AddObject(item);
      SOUpdated(SteamID, item);
    }
  }

  public void UpdateKnifeSkin(WeaponSkinData skinData)
  {
    unsafe {
      var item = NativeService.CreateCEconItemInstance();
      ref var loadout = ref Loadouts[skinData.Team, loadout_slot_t.LOADOUT_SLOT_MELEE];
      if (IsValidItemID(loadout.ItemId))
      {
        var oldItem = GetEconItemByItemID(loadout.ItemId);
        if (oldItem == null)
        {
          throw new Exception($"GetEconItemByItemID returned null for item id {loadout.ItemId}");
        }
        item.AccountID = oldItem.AccountID;
        item.ItemID = oldItem.ItemID;
        SOCache.RemoveObject(oldItem);
        SODestroyed(SteamID, oldItem);
      } else {
        item.AccountID = new CSteamID(SteamID).GetAccountID().m_AccountID;
        // not sure if it will work
        item.ItemID = (ulong)DateTime.UtcNow.Ticks;
        UpdateLoadoutItem(skinData.Team, skinData.DefinitionIndex, item.ItemID);
      }
      item.Apply(skinData);
      SOCache.AddObject(item);
      SODestroyed(SteamID, item);
    }
  }
}