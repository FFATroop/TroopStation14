using Robust.Shared.GameStates;

namespace Content.Shared.Eye
{
    [NetworkedComponent]
    public abstract partial class SharedDarkVisionComponent : Component
    {
        [DataField("shaderTexture")]
        public String? ShaderTexturePrototype;

        [DataField("layerColor")]
        public Color LayerColor = Color.FromHex("#00000000");

        [DataField("mustDrawLight")]
        public bool DrawLight = true;

        [DataField("mustDrawFOV")]
        public bool DrawFOV = true;

        [DataField("toggle")]
        public bool IsEnable = false;
    }
}
