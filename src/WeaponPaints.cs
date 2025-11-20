using Microsoft.Extensions.DependencyInjection;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Schemas;

namespace WeaponPaints;

[PluginMetadata(Id = "WeaponPaints", Version = "0.1.0", Name = "WeaponPaints", Author = "samyyc", Description = "No description.")]
public partial class WeaponPaints : BasePlugin {
  public WeaponPaints(ISwiftlyCore core) : base(core)
  {
  }

  public override void Load(bool hotReload) {
    
  }

  public override void Unload() {
  }
} 