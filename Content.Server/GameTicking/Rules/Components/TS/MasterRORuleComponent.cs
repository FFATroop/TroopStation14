

using Robust.Shared.Map;
using System.Numerics;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class MasterRORuleComponent : Component
{
    public MapId? MissionMapId;
    public MapId? MainStationMapId;
    public Vector2 CurrentCenterMissionMap;
    public EntityUid? MainStationUid;
    public EntityUid? MainShuttleUid;

    public bool IsAntagSpawned = false;

    public List<EntityUid> SpawnedMissionEntities = new List<EntityUid>();

    public WinMissionType WinType = WinMissionType.GarrisonMinorLose;

    public List<WinMissionCondition> WinConditions = new();
}


public enum WinMissionType : byte
{
    GarrisonMajorWin,

    GarrisonMinorWin,

    GarrisonMinorLose,

    GarrisonMajorLose
}


public enum WinMissionCondition : byte
{
    AllAntagsDead,
    AllCrewDead,

    SomeAntagsAlive,
    SomeCrewAlive,

    ShuttleLost,
    AntagsOnMainStation,

    SomeObjectivesComplete,
    AllObjectivesComplete,

    EvacuationShuttleCalled,
    MainStationExplode
}
