
using System.Linq;
using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.TS;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;


namespace Content.Server.GameTicking.Rules;

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

    protected override void Started(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        var randDelay = 60000; //_random.Next(400000, 600000);
        _timerManager.AddTimer(new Timer(randDelay, false, startEvent));
    }

    private void startEvent()
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
        var mapId = new MapId(smallestValue);

        var allFoundMaps = _prototypeManager.EnumeratePrototypes<MissionMapPrototype>().ToList();

        if (!allFoundMaps.Any())
        {
            _logManager.GetSawmill("RORule").Error("Event cant spawn map, because cant find any mission map prototype");
            return;
        }

        var allFoundGrids = _prototypeManager.EnumeratePrototypes<MissionGridPrototype>().ToList();

        _chat.DispatchGlobalAnnouncement("research-mission-message", "research-mission-sender", true, null, Color.Blue);

        var indexMap = _random.Next(allFoundMaps.Count() - 1);

        if (!_mapLoader.TryLoad(mapId, allFoundMaps[indexMap].MapPath.ToString(), out var entityList))
        {
            return;
        }

        var grids = _mapManager.GetAllMapGrids(mapId);

        if (!grids.Any())
        {
            _logManager.GetSawmill("RORule").Error("Event cant found main grid!");
            return;
        }
        var entityMap = _mapManager.GetMapEntityId(mapId);

        var centerMap = _mapSystem.LocalToWorld(entityMap, grids.First(), Vector2.Zero);

        var ftlPoint = _entMan.SpawnEntity("FTLPointUnknown", new MapCoordinates(centerMap, mapId));

        int objectCount = _random.Next(6);
        float angleDelta = 360 / objectCount;
        var xDelta = _random.NextFloat(0.5f, 1);
        var startVector = new Vector2(xDelta, 1 - (xDelta * xDelta));   // hard math of point on normalized circle, Y found by Pifagor's theorem
        for (var i = 0; i < objectCount; ++i)
        {
            var indexGrid = _random.Next(allFoundGrids.Count() - 1);
            float currentAngle = angleDelta * i;
            var tempOptions = new MapLoadOptions();
            tempOptions.Offset = new Vector2(
                (startVector.X * Single.Cos(currentAngle) - startVector.Y * Single.Sin(currentAngle) ),
                (startVector.X * Single.Sin(currentAngle) + startVector.Y * Single.Cos(currentAngle) )
                ) * _random.NextFloat(140f, 220f) + centerMap;
            tempOptions.Rotation = _random.NextAngle(0, 360);

            if (!_mapLoader.TryLoad(mapId, allFoundGrids[indexGrid].GridPath.ToString(), out  _, tempOptions)) continue;
        }
        _mapManager.DoMapInitialize(mapId);
    }


    protected override void ActiveTick(EntityUid uid, MasterRORuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        //_chat.DispatchGlobalAnnouncement("Added gamerule", "SpecialForces", true, null, Color.BetterViolet);
    }
}
