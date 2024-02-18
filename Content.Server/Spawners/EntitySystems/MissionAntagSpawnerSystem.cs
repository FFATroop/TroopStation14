using System.Linq;
using Content.Server.GameTicking.Rules;
using Content.Server.Spawners.Components;
using Content.Server.TS;
using Content.Shared.Ghost;
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

            SubscribeLocalEvent<MissionMapInitEventArgs>(OnMissionMapInit);
        }

        private void OnMissionMapInit(MissionMapInitEventArgs args)
        {
            // over 3 min after map init
            _timerManager.AddTimer(new Timer(180000,false, DelayedCalculationRoleBalance));
        }

        private void DelayedCalculationRoleBalance()
        {
            var playerWhithoutEntityCount = 0;
            var playerGhostCount = 0;
            foreach (var player in _playerManager.Sessions)
            {
                if (player.AttachedEntity == null)
                {
                    ++playerWhithoutEntityCount;
                    continue;
                }
                if (TryComp<GhostComponent>(player.AttachedEntity, out _))
                {
                    ++playerGhostCount;
                    continue;
                }
                ++_playersRoundstartCount;
            }
            var possibleAntagPools = _prototypeManager.EnumeratePrototypes<AntagPoolsPrototype>().ToList();
            if (!possibleAntagPools.Any())
            {
                _logManager.GetSawmill("MissionAntagSpawner")
                    .Error("Not presented any AntagPoolsPrototype!");
                return;
            }
            var currentAntagPool = possibleAntagPools[_random.Next(possibleAntagPools.Count() - 1)];
            if (!currentAntagPool.Pools.Any())
            {
                _logManager.GetSawmill("MissionAntagSpawner")
                    .Error("{0} has not any AntagRolesPoolPrototype in pool!", currentAntagPool.ID);
                return;
            }
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

            _logManager.GetSawmill("MissionAntagSpawner").Info(
                    "Calculating balance is over, active player count = {0}, antags count = {1}, boss count = {2}, ghost count = {3}, player without entity = {4}",
                    _playersRoundstartCount, _balancedAntagsCount, _balancedBossAntagsCount, playerGhostCount, playerWhithoutEntityCount);

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

            while (tempAntags < _balancedAntagsCount || tempBoss < _balancedBossAntagsCount)
            {
                var tempEntity = spawnerEntities[_random.Next(spawnerEntities.Count() - 1)];

                if (tempEntity.Comp.Chance < 0.999f && !_robustRandom.Prob(tempEntity.Comp.Chance))
                {
                    // Every chance misses, adding 0.3f value to next chance check
                    // it means we will get 100% chance on every component in last check
                    // and never get infinity loop, also we save percentage chance to first loop
                    // if we had only 2 50% chances spawners and must place only 2 entities
                    // we must use all spawners without probability, and we guarantied to loss probability in next loops after first
                    tempEntity.Comp.Chance += 0.3f;
                    return;
                }

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
            }
        }
    }
}
