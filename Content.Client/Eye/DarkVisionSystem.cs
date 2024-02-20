using Content.Shared.Eye;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Eye;

public sealed class DarkVisionSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DarkVisionComponent, ComponentInit>(OnVisionInit);
        SubscribeLocalEvent<DarkVisionComponent, ComponentShutdown>(OnVisionShutdown);
        SubscribeLocalEvent<DarkVisionComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<DarkVisionComponent, PlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnPlayerAttached(EntityUid uid, DarkVisionComponent component, PlayerAttachedEvent args)
    {
        if (!component.IsEnable)
            return;

        if (_player.LocalSession == null)
            return;
        if (_player.LocalSession.AttachedEntity != uid)
            return;

        if (component.ShaderTexturePrototype != null)
        {
            if (component.darkOverlay == null)
                component.darkOverlay = new DarkVisionOverlay();
            component.darkOverlay.SetShaderProto(component.ShaderTexturePrototype);
            _lightManager.DrawLighting = component.DrawLight;
            _overlayMan.AddOverlay(component.darkOverlay);
        }
    }

    private void OnPlayerDetached(EntityUid uid, DarkVisionComponent component, PlayerDetachedEvent args)
    {
        if (!component.IsEnable)
            return;

        if (_player.LocalSession == null)
            return;
        if (_player.LocalSession.AttachedEntity != uid)
            return;

        if (component.darkOverlay != null)
        {
            _overlayMan.RemoveOverlay(component.darkOverlay);
            component.darkOverlay = null;
        }

        _lightManager.DrawLighting = true;
    }

    private void OnVisionInit(EntityUid uid, DarkVisionComponent component, ComponentInit args)
    {
        if (_player.LocalSession == null)
            return;
        if (_player.LocalSession.AttachedEntity != uid)
            return;

        _lightManager.DrawLighting = component.DrawLight;
        if (component.darkOverlay == null)
            component.darkOverlay = new DarkVisionOverlay();
        _overlayMan.AddOverlay(component.darkOverlay);
    }

    private void OnVisionShutdown(EntityUid uid, DarkVisionComponent component, ComponentShutdown args)
    {
        if (!component.IsEnable)
            return;

        if (_player.LocalSession == null)
            return;
        if (_player.LocalSession.AttachedEntity != uid)
            return;

        _lightManager.DrawLighting = true;
        if (component.darkOverlay != null)
            _overlayMan.RemoveOverlay(component.darkOverlay);
        
    }
}
