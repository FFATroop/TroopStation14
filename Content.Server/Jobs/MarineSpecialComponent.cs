using Content.Server.Animals.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Jobs;

/// <summary>
///     Lets an entity produce wool fibers. Uses hunger if present.
/// </summary>

[RegisterComponent]
public sealed partial class MarineSpecialComponent : Component;
