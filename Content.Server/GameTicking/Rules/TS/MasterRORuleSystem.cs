using System.Linq;
using System.Numerics;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Content.Server.RoundEnd;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Components;
using Content.Server.TS;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
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
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly ITimerManager _timerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ILocalizationManager _localizationManager = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagCalculatedAndSpawnedEventArgs>(OnAntagsSpawned);
    }

    protected override void Started(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        //base.Started(uid, component, gameRule, args);
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
        component.MissionMapId = new MapId(smallestValue);


        var allFoundMaps = _prototypeManager.EnumeratePrototypes<MissionMapPrototype>().ToList();

        if (!allFoundMaps.Any())
        {
            _logManager.GetSawmill("RORule").Error("Event cant spawn map, because cant find any mission map prototype");
            return;
        }

        var allFoundGrids = _prototypeManager.EnumeratePrototypes<MissionGridPrototype>().ToList();
        var indexMap = _random.Next(allFoundMaps.Count() - 1);

        if (!_mapLoader.TryLoad(component.MissionMapId.Value, allFoundMaps[indexMap].MapPath.ToString(), out var entityList))
        {
            return;
        }

        var grids = _mapManager.GetAllMapGrids(component.MissionMapId.Value);
        var mainGrid = grids.First();

        if (!grids.Any())
        {
            _logManager.GetSawmill("RORule").Error("Event cant found main grid!");
            return;
        }
        var entityMap = _mapManager.GetMapEntityId(component.MissionMapId.Value);

        component.CurrentCenterMissionMap = _mapSystem.LocalToWorld(entityMap, mainGrid, Vector2.Zero);

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
                ) * _random.NextFloat(140f, 220f) + component.CurrentCenterMissionMap;
            tempOptions.Rotation = _random.NextAngle(0, 360);

            _mapLoader.TryLoad(component.MissionMapId.Value, allFoundGrids[indexGrid].GridPath.ToString(), out _, tempOptions);
        }
        _mapManager.DoMapInitialize(component.MissionMapId.Value);

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
                if (tempTransform.MapID != component.MissionMapId.Value) continue;
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
        var guaranteedPointsCount = 1;
        var sidePointsCount = 2;
        int debugCountItems = 0;

        var tempPoolIndex = _random.Next(allSecretPools.Count() - 1);

        var tempTupleSpawnerList = new List<Tuple<EntityUid, bool>>();
        foreach (var tempMissionItem in missionItemSpawners)
        {
            tempTupleSpawnerList.Add(new Tuple<EntityUid, bool>(tempMissionItem, false));
        }

        if (missionItemSpawners.Any())
        {
            if (missionItemSpawners.Count() <= guaranteedPointsCount)
            {
                foreach (var tempMissionItem in missionItemSpawners)
                {
                    if (spawnMissionItem(allSecretPools[tempPoolIndex], tempMissionItem, entityMap, mainGrid, component))
                        ++debugCountItems;
                }
            }
            else
            {
                for (var i = 0; i < guaranteedPointsCount; ++i)
                {
                    var tempRandMissionIndex = _random.Next(missionItemSpawners.Count());

                    if (spawnMissionItem(allSecretPools[tempPoolIndex], missionItemSpawners[tempRandMissionIndex],
                            entityMap, mainGrid, component))
                    {
                        missionItemSpawners.Remove(missionItemSpawners[tempRandMissionIndex]);
                        ++debugCountItems;
                    }
                }


                if (missionItemSpawners.Count() <= sidePointsCount)
                {
                    foreach (var tempMissionItem in missionItemSpawners)
                    {
                        if (spawnMissionItem(allSecretPools[tempPoolIndex], tempMissionItem, entityMap, mainGrid, component))
                            ++debugCountItems;
                    }
                }
                else
                {
                    for (var i = 0; i < sidePointsCount; ++i)
                    {
                        var tempRandMissionIndex = _random.Next(missionItemSpawners.Count());
                        if (spawnMissionItem(allSecretPools[tempPoolIndex], missionItemSpawners[tempRandMissionIndex],
                                entityMap, mainGrid, component))
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


    protected override void ActiveTick(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule,
        float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        // fastest implementation without some functionality
        // @todo refactor this

        if (!component.IsAntagSpawned || component.MainStationUid == null || component.MainShuttleUid == null)
            return;

        MapId? mainMapId = null;
        if (TryComp<TransformComponent>(component.MainStationUid, out var xformMain))
        {
            mainMapId = xformMain.MapID;
        }

        bool isAllItems = true;
        bool isSomeItems = false;
        foreach (var missionItem in component.SpawnedMissionEntities)
        {
            if (TryComp<TransformComponent>(missionItem, out var xform))
            {
                if (xform.MapID == mainMapId && mainMapId != null)
                {
                    isSomeItems = true;
                }
                else
                {
                    isAllItems = false;
                }
            }
        }

        bool isAllAntagDead = true;
        bool isSomeAntagDead = false;
        bool isSomeAntagOnMainMap = false;
        var query = EntityQueryEnumerator<MissionAntagComponent, TransformComponent>();
        while (query.MoveNext(out var eq_uid, out var eq_component, out var eq_xform))
        {
            if (_mobStateSystem.IsAlive(eq_uid))
            {
                if (eq_xform.MapID == mainMapId && mainMapId != null)
                {
                    isSomeAntagOnMainMap = true;
                }

                if (component.MainShuttleUid != null)
                {
                    if (eq_xform.GridUid == component.MainShuttleUid)
                        isSomeAntagOnMainMap = true;
                }

                isAllAntagDead = false;
            }
            else
            {
                isSomeAntagDead = true;
            }
        }

        if (isAllAntagDead && isAllItems)
        {
            component.WinConditions.Add(WinMissionCondition.AllAntagsDead);
            component.WinConditions.Add(WinMissionCondition.AllObjectivesComplete);
            component.WinType = WinMissionType.GarrisonMajorWin;
            _roundEndSystem.EndRound();
        }
    }


    private void startEvent()
    {
        var query = EntityQueryEnumerator<MasterRORuleComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.MissionMapId == null) continue;
            var ftlPoint = _entMan.SpawnEntity("FTLPointUnknown",
                new MapCoordinates(component.CurrentCenterMissionMap, component.MissionMapId.Value));
        }
        var senderLocale = _localizationManager.GetString("research-mission-sender");
        var messageLocale = _localizationManager.GetString("research-mission-message");

        _chat.DispatchGlobalAnnouncement(messageLocale, senderLocale, true, null, Color.GreenYellow);
    }

    private void OnAntagsSpawned(AntagCalculatedAndSpawnedEventArgs args)
    {
        var query = EntityQueryEnumerator<MasterRORuleComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            component.IsAntagSpawned = true;

            var stationQuery = EntityQueryEnumerator<TargetStationComponent>();
            while (stationQuery.MoveNext(out var targetStation))
            {
                // @todo refactor for many stations
                component.MainStationUid = targetStation.Owner;
                break;
            }

            var shuttlesQuery = EntityQueryEnumerator<TargetShuttleComponent>();
            while (shuttlesQuery.MoveNext(out var targetShuttle))
            {
                // @todo refactor for many stations
                component.MainShuttleUid = targetShuttle.Owner;
                break;
            }

            if (component.MainStationUid == null)
            {
                _logManager.GetSawmill("RORule").Error("Event cant found any target station!");
            }

            if (component.MainShuttleUid == null)
            {
                _logManager.GetSawmill("RORule").Error("Event cant found any target shuttle!");
            }
        }

        _logManager.GetSawmill("RORule").Info("Shuttle and Station initialized in game rule!");
    }

    private bool spawnMissionItem(SecretPoolPrototype secretPool, EntityUid missionItemSpawner, EntityUid entityMap, MapGridComponent mainGrid, MasterRORuleComponent component)
    {
        if (!TryComp(missionItemSpawner, out TransformComponent? xComp))
        {
            _logManager.GetSawmill("RORule")
                .Error(
                    "Item {0} has no transform component, cant spawn random guaranteed mission item!", missionItemSpawner.Id);
            return false;
        }

        if (component.MissionMapId == null)
            return false;

        var tempItemPoolIndex = _random.Next(secretPool.PoolItems.Count() - 1);
        var spawnedItem = _entMan.Spawn(
            secretPool.PoolItems[tempItemPoolIndex],
            new MapCoordinates(_mapSystem.LocalToWorld(entityMap, mainGrid, xComp.LocalPosition),
                component.MissionMapId.Value)
        );
        component.SpawnedMissionEntities.Add(spawnedItem);
        return true;
    }
}
