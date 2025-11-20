using SwiftlyS2.Shared.Players;

namespace WeaponPaints;

public record WeaponSkinData(
  Team Team,
  ushort DefinitionIndex,
  int Paintkit,
  int PaintkitSeed,
  float PaintkitWear
);