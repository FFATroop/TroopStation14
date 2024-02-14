using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.TS;

/// <summary>
/// Stores data for generic queries.
/// Each query is run in turn to get the final available results.
/// These results are then run through the considerations.
/// </summary>
[Prototype]
public sealed partial class AntagRolesPoolPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<String> PoolDefaultEntities = new();

    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<String> BossPools = new();

    [DataField]
    public float AntagsPerPlayer = 1.5f;

    [DataField]
    public int MinBossCount = 1;

    [DataField]
    public int MaxBossCount = 1;

    // Coefficient used to calc boss count from MinBossCount to MaxBossCount
    [DataField]
    public float BossPerPlayer = 0.1f;
}
