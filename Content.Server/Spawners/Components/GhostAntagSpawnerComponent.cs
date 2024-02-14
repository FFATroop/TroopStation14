using Robust.Shared.Prototypes;

namespace Content.Server.Spawners.Components
{
    [RegisterComponent]
    [Virtual]
    public partial class GhostAntagSpawnerComponent : Component
    {
        [DataField]
        public float Chance { get; set; } = 1.0f;
    }
}
