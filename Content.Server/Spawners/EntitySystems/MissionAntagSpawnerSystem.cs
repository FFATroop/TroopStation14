using System.Linq;
using System.Numerics;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Spawners.Components;
using Content.Server.TS;
using Content.Shared.Ghost;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Spawners.EntitySystems
{
    [UsedImplicitly]
    public sealed class MissionAntagSpawnerSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly ITimerManager _timerManager = default!;
        [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
        [Dependency] private readonly ILogManager _logManager = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private int _balancedAntagsCount = 0;
        private int _playersRoundstartCount = 0;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MapInitEvent>(OnSpawnMapInit);
        }

        private void OnSpawnMapInit(MapInitEvent args)
        {
            // over 1 min after map init
            _timerManager.AddTimer(new Timer(60000,false, DelayedCalculationRoleBalance));
        }

        private void DelayedCalculationRoleBalance()
        {
            foreach (var player in _playerManager.Sessions)
            {
                if (player.AttachedEntity == null) continue;
                if (!TryComp<GhostComponent>(player.AttachedEntity, out _)) continue;
                ++_playersRoundstartCount;
            }
            _balancedAntagsCount = (int)(_playersRoundstartCount * 1.5f);

            var possibleAntagPools = _prototypeManager.EnumeratePrototypes<AntagPoolsPrototype>().ToList();

            if (!possibleAntagPools.Any()) return;

            SpawnAllGhostedRoles(possibleAntagPools[_random.Next(possibleAntagPools.Count() - 1)]);
        }

        private void SpawnAllGhostedRoles(AntagPoolsPrototype antagPools)
        {
            var query = EntityQueryEnumerator<GhostAntagSpawnerComponent>();
            while (query.MoveNext(out var uid, out var spawner))
            {

            }
        }

        private void Spawn(EntityUid uid, GhostAntagSpawnerComponent component)
        {
            if (component.Chance != 1.0f && !_robustRandom.Prob(component.Chance))
                return;

            if (!Deleted(uid))
                EntityManager.SpawnEntity(_robustRandom.Pick(component.Prototypes), Transform(uid).Coordinates);
        }
    }
}
