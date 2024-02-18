using Content.Shared.Eye;
using Robust.Shared.GameStates;

namespace Content.Server.Eye
{
    [RegisterComponent]
    [NetworkedComponent]
    public sealed partial class DarkVisionComponent : SharedDarkVisionComponent { }
}
