using Content.Shared.Eye;
using Robust.Shared.GameStates;

namespace Content.Client.Eye
{
    [RegisterComponent]
    [NetworkedComponent]
    public sealed partial class DarkVisionComponent : SharedDarkVisionComponent
    {
        public DarkVisionOverlay? darkOverlay = null;

    }
}
