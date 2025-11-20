using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;

namespace WeaponPaints.Inventory;

public class InventoryService
{
  private ISwiftlyCore Core { get; init; }
  private NativeService NativeService { get; init; }
  private ILogger<InventoryService> Logger { get; init; }

  private Dictionary<ulong /* steamid */, CCSPlayerInventory /* inventory */> SubscribedInventories = new();

  public InventoryService(ISwiftlyCore core, NativeService nativeService, ILogger<InventoryService> logger)
  {
    Core = core;
    NativeService = nativeService;
    Logger = logger;

    NativeService.OnSOCacheSubscribed += OnSOCacheSubscribed;

    Core.Event.OnClientDisconnected += OnClientDisconnected;
  }

  private void OnSOCacheSubscribed(CCSPlayerInventory pInventory, SOID_t pSOID)
  {
    Logger.LogInformation($"SOCacheSubscribed: {pSOID.SteamID}");
    SubscribedInventories[pSOID.SteamID] = pInventory;
  }

  private void OnClientDisconnected(IOnClientDisconnectedEvent @event)
  {
    var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
    if (player == null)
    {
      return;
    }
    SubscribedInventories.Remove(player.SteamID);
  }
}