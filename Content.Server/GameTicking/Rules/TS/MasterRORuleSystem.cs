using System.Linq;
using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.TS;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.GameTicking.Rules;

public sealed class MissionMapInitEventArgs : EventArgs { }

public sealed class MasterRORuleSystem : GameRuleSystem<MasterRORuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly ITimerManager _timerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ILocalizationManager _localizationManager = default!;

    private static MapId _currentMissionMapId;
    private static Vector2 _currentCenterMissionMap;
    protected override void Started(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        var smallestValue = 1;
        for (; smallestValue < 25; ++smallestValue)             // 25 is magic number
        {
            if (_mapManager.MapExists(new MapId(smallestValue)))
                continue;
            break;
        }

        if (smallestValue == 25)
        {
            _logManager.GetSawmill("RORule").Error("Event cant spawn map, because maps already more than 24");
            return;
        }
        _currentMissionMapId = new MapId(smallestValue);

        var allFoundMaps = _prototypeManager.EnumeratePrototypes<MissionMapPrototype>().ToList();

        if (!allFoundMaps.Any())
        {
            _logManager.GetSawmill("RORule").Error("Event cant spawn map, because cant find any mission map prototype");
            return;
        }

        var allFoundGrids = _prototypeManager.EnumeratePrototypes<MissionGridPrototype>().ToList();
        var indexMap = _random.Next(allFoundMaps.Count() - 1);

        if (!_mapLoader.TryLoad(_currentMissionMapId, allFoundMaps[indexMap].MapPath.ToString(), out var entityList))
        {
            return;
        }

        var grids = _mapManager.GetAllMapGrids(_currentMissionMapId);
        var mainGrid = grids.First();

        if (!grids.Any())
        {
            _logManager.GetSawmill("RORule").Error("Event cant found main grid!");
            return;
        }
        var entityMap = _mapManager.GetMapEntityId(_currentMissionMapId);

        _currentCenterMissionMap = _mapSystem.LocalToWorld(entityMap, mainGrid, Vector2.Zero);

        int objectCount = _random.Next(5, 9);
        _logManager.GetSawmill("RORule").Info("Side grid count = {0}", objectCount);

        float angleDelta = 360 / objectCount;
        var xDelta = _random.NextFloat(0.5f, 1);
        var startVector = new Vector2(xDelta, 1 - (xDelta * xDelta));   // hard math of point on normalized circle, Y found by Pifagor's theorem
        for (var i = 0; i < objectCount; ++i)
        {
            var indexGrid = _random.Next(allFoundGrids.Count() - 1);
            float currentAngle = angleDelta * i;
            var tempOptions = new MapLoadOptions();
            tempOptions.Offset = new Vector2(
                (startVector.X * Single.Cos(currentAngle) - startVector.Y * Single.Sin(currentAngle)),
                (startVector.X * Single.Sin(currentAngle) + startVector.Y * Single.Cos(currentAngle))
                ) * _random.NextFloat(140f, 220f) + _currentCenterMissionMap;
            tempOptions.Rotation = _random.NextAngle(0, 360);

            _mapLoader.TryLoad(_currentMissionMapId, allFoundGrids[indexGrid].GridPath.ToString(), out _, tempOptions);
        }
        _mapManager.DoMapInitialize(_currentMissionMapId);

        var allSecretPools = _prototypeManager.EnumeratePrototypes<SecretPoolPrototype>().ToList();

        if (!allSecretPools.Any())
        {
            _logManager.GetSawmill("RORule").Error("No one secretPoolPrototype found!");
            return;
        }

        var missionItemSpawners = new List<EntityUid>();
        var tempEntities = _entMan.GetEntities();

        foreach (var tempEnt in tempEntities)     // there is no other way to get entity by ID in current map
        {
            if (_entMan.TryGetComponent<TransformComponent>(tempEnt, out var tempTransform))
            {
                if (tempTransform.MapID != _currentMissionMapId) continue;
            }

            if (_entMan.TryGetComponent<MetaDataComponent>(tempEnt, out var tempMeta))
            {
                if (tempMeta.EntityPrototype == null)
                    continue;

                if (tempMeta.EntityPrototype.ID == "MissionItemSpawn")
                {
                    missionItemSpawners.Add(tempEnt);
                }
            }
        }

        if (!missionItemSpawners.Any())
        {
            _logManager.GetSawmill("RORule").Error("No one MissionItemPrototype found!");
            return;
        }
        else
            _logManager.GetSawmill("RORule").Info("Found {0} mission item's spawners on RO map", missionItemSpawners.Count());

        // @todo antag player system balance
        var guaranteedPointsCount = 3;
        var sidePointsCount = 2;
        int debugCountItems = 0;

        var tempPoolIndex = _random.Next(allSecretPools.Count() - 1);

        var tempTupleSpawnerList = new List<Tuple<EntityUid, bool>>();
        foreach (var tempMissionItem in missionItemSpawners)
        {
            tempTupleSpawnerList.Add(new Tuple<EntityUid, bool>(tempMissionItem, false));
        }

        if (missionItemSpawners.Count() <= guaranteedPointsCount)
        {
            foreach (var tempMissionItem in missionItemSpawners)
            {
                if (spawnMissionItem(allSecretPools[tempPoolIndex], tempMissionItem, entityMap, mainGrid))
                    ++debugCountItems;
            }
        }
        else
        {
            for (var i = 0; i < guaranteedPointsCount; ++i)
            {
                var tempRandMissionIndex = _random.Next(missionItemSpawners.Count());

                if (spawnMissionItem(allSecretPools[tempPoolIndex], missionItemSpawners[tempRandMissionIndex],
                        entityMap, mainGrid))
                {
                    missionItemSpawners.Remove(missionItemSpawners[tempRandMissionIndex]);
                    ++debugCountItems;
                }
            }

            if (missionItemSpawners.Any())
            {
                if (missionItemSpawners.Count() <= sidePointsCount)
                {
                    foreach (var tempMissionItem in missionItemSpawners)
                    {
                        if (_random.Next(100) > 60)
                            continue; // 40% chance

                        if (spawnMissionItem(allSecretPools[tempPoolIndex], tempMissionItem, entityMap, mainGrid))
                            ++debugCountItems;
                    }
                }
                else
                {
                    for (var i = 0; i < sidePointsCount; ++i)
                    {
                        var tempRandMissionIndex = _random.Next(missionItemSpawners.Count());
                        if (spawnMissionItem(allSecretPools[tempPoolIndex], missionItemSpawners[tempRandMissionIndex],
                                entityMap, mainGrid))
                        {
                            missionItemSpawners.Remove(missionItemSpawners[tempRandMissionIndex]);
                            ++debugCountItems;
                        }
                    }
                }
            }
        }
        RaiseLocalEvent(new MissionMapInitEventArgs());
        _logManager.GetSawmill("RORule").Info("Spawned {0} mission items on RO map", debugCountItems);

        var randDelay = _random.Next(300000, 500000); // 5 - 8.33 min
        _timerManager.AddTimer(new Timer(randDelay, false, startEvent));
    }


    protected override void ActiveTick(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        //_chat.DispatchGlobalAnnouncement("Added gamerule", "SpecialForces", true, null, Color.BetterViolet);
    }


    private void startEvent()
    {
        var ftlPoint = _entMan.SpawnEntity("FTLPointUnknown", new MapCoordinates(_currentCenterMissionMap, _currentMissionMapId));

        var senderLocale = _localizationManager.GetString("research-mission-sender");
        var messageLocale = _localizationManager.GetString("research-mission-message");

        _chat.DispatchGlobalAnnouncement(messageLocale, senderLocale, true, null, Color.GreenYellow);
    }


    private bool spawnMissionItem(SecretPoolPrototype secretPool, EntityUid missionItemSpawner, EntityUid entityMap, MapGridComponent mainGrid)
    {
        if (!TryComp(missionItemSpawner, out TransformComponent? xComp))
        {
            _logManager.GetSawmill("RORule")
                .Error(
                    "Item {0} has no transform component, cant spawn random guaranteed mission item!", missionItemSpawner.Id);
            return false;
        }

        var tempItemPoolIndex = _random.Next(secretPool.PoolItems.Count() - 1);
        _entMan.Spawn(
            secretPool.PoolItems[tempItemPoolIndex],
            new MapCoordinates(_mapSystem.LocalToWorld(entityMap, mainGrid, xComp.LocalPosition),
                _currentMissionMapId)
        );
        return true;
    }




}
