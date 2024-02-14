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
using FastAccessors;
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
        [Dependency] private readonly ITimerManager _timerManager = default!;
        [Dependency] private readonly ILogManager _logManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private int _balancedAntagsCount = 0;
        private int _balancedBossAntagsCount = 0;
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
            var possibleAntagPools = _prototypeManager.EnumeratePrototypes<AntagPoolsPrototype>().ToList();
            if (!possibleAntagPools.Any()) return;
            var currentAntagPool = possibleAntagPools[_random.Next(possibleAntagPools.Count() - 1)];

            var currentPool = _prototypeManager.Index<AntagRolesPoolPrototype>(currentAntagPool.Pools[currentAntagPool.Pools.Count() - 1]);

            _logManager.GetSawmill("MissionAntagSpawner")
                .Info("Selected antag pool to {0}", currentPool.ID);

            _balancedAntagsCount = (int) (_playersRoundstartCount * currentPool.AntagsPerPlayer);

            if (currentPool.MinBossCount == currentPool.MaxBossCount || currentPool.MinBossCount > currentPool.MaxBossCount)
                _balancedBossAntagsCount = currentPool.MaxBossCount;

            var bossCount = (int)Math.Ceiling(currentPool.BossPerPlayer * _playersRoundstartCount);
            if (bossCount >= currentPool.MaxBossCount) _balancedBossAntagsCount = currentPool.MaxBossCount;
            else if (bossCount <= currentPool.MinBossCount) _balancedBossAntagsCount = currentPool.MinBossCount;
            else _balancedBossAntagsCount = bossCount;

            _logManager.GetSawmill("MissionAntagSpawner")
                .Info("Calculating balance is over, active player count = {0}, antags count = {1}, boss count = {2}",
                    _playersRoundstartCount, _balancedAntagsCount, _balancedBossAntagsCount);

            SpawnAllGhostedRoles(currentPool);
        }

        private void SpawnAllGhostedRoles(AntagRolesPoolPrototype antagPools)
        {
            var query = EntityQueryEnumerator<GhostAntagSpawnerComponent>();
            int tempAntags = 0;
            int tempBoss = 0;
            var spawnerEntities = new List<Entity<GhostAntagSpawnerComponent>>();
            while (query.MoveNext(out var uid, out var spawner))
            {
                spawnerEntities.Add(new Entity<GhostAntagSpawnerComponent>(uid, spawner));
            }

            if (!spawnerEntities.Any())
            {
                _logManager.GetSawmill("MissionAntagSpawner")
                    .Error("Not presented any GhostAntagSpawner!");
                return;
            }

            while (tempAntags < _balancedAntagsCount && tempBoss < _balancedBossAntagsCount)
            {
                var tempEntity = spawnerEntities[_random.Next(spawnerEntities.Count() - 1)];

                if (tempEntity.Comp.Chance != 1.0f && !_robustRandom.Prob(tempEntity.Comp.Chance))
                    return;

                if (Deleted(tempEntity.Owner))
                    return;

                if (tempAntags < _balancedAntagsCount)
                {
                    var antagEntity = EntityManager.SpawnEntity(_robustRandom.Pick(antagPools.PoolDefaultEntities),
                        Transform(tempEntity.Owner).Coordinates);
                    if (antagEntity.IsValid())
                        ++tempAntags;
                }
                else if (tempBoss < _balancedBossAntagsCount)
                {
                    var bossEntity = EntityManager.SpawnEntity(_robustRandom.Pick(antagPools.BossPools),
                        Transform(tempEntity.Owner).Coordinates);
                    if (bossEntity.IsValid())
                        ++tempBoss;
                }

                foreach (var tempSpawner in spawnerEntities)
                {
                    QueueDel(tempSpawner.Owner);
                }
            }

        }

    }
}
