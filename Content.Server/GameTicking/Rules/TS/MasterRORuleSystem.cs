
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules.Components;


namespace Content.Server.GameTicking.Rules;

public sealed class MasterRORuleSystem : GameRuleSystem<MasterRORuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;

    protected override void Added(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        _chat.DispatchGlobalAnnouncement("Added gamerule", "SpecialForces", true, null, Color.BetterViolet);
    }


    protected override void Started(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        _chat.DispatchGlobalAnnouncement("Started gamerule", "SpecialForces", true, null, Color.BetterViolet);
    }

    /// <summary>
    /// Called when the gamerule ends
    /// </summary>
    protected override void Ended(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        _chat.DispatchGlobalAnnouncement("Ended gamerule", "SpecialForces", true, null, Color.BetterViolet);
    }

    /// <summary>
    /// Called on an active gamerule entity in the Update function
    /// </summary>
    protected override void ActiveTick(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        //_chat.DispatchGlobalAnnouncement("Added gamerule", "SpecialForces", true, null, Color.BetterViolet);
    }
}
