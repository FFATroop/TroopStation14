

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class MasterRORuleComponent : Component
{
}


public enum WinMissionType : byte
{
    GarrisonMajorWin,

    GarrisonMinorWin,

    Neutral,

    GarrisonMinorLose,

    GarrisonMajorLose
}


public enum WinMissionCondition : byte
{
    AllAntagsDead,

    AllCrewDead,
    NukeExplodedOnCorrectStation,
    NukeExplodedOnNukieOutpost,
    NukeExplodedOnIncorrectLocation,
    NukeActiveInStation,
    NukeActiveAtCentCom,
    NukeDiskOnCentCom,
    NukeDiskNotOnCentCom,
    NukiesAbandoned,
    AllNukiesDead,
    SomeNukiesAlive,
    AllNukiesAlive
}
