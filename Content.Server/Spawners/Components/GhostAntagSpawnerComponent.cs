using Robust.Shared.Prototypes;

namespace Content.Server.Spawners.Components
{
    [RegisterComponent]
    [Virtual]
    public partial class GhostAntagSpawnerComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public float Chance { get; set; } = 1.0f;
    }
}
