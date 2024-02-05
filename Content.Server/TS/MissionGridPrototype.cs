using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Robust.Shared.Utility;

namespace Content.Server.TS;

/// <summary>
/// Stores data for generic queries.
/// Each query is run in turn to get the final available results.
/// These results are then run through the considerations.
/// </summary>
[Prototype]
public sealed partial class MissionGridPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ResPath GridPath;
}
