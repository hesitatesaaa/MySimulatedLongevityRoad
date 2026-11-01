using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal sealed class MclslCareerCast
{
    public int Tier, Phase;
    public long Target;
    public double Due;
}
internal sealed class MclslCareerZone
{
    public int Tier, X, Y;
    public double Expires, Pulse;
}
internal sealed class MclslCareerSummon
{
    public long Unit;
    public int Kind;
    public double Expires, Pulse;
}
internal sealed class MclslCareerMark
{
    public int SwordHits, Sigils;
    public double Expires, OwnerHit, SpiritHit, NextAssist;
}
internal sealed class MclslCareerBattleState
{
    public int Version=2;
    public long Owner;
    public int Career, Sword, Rage, Elements, LastElement;
    public bool Counter, FullCycle;
    public double LastAction, NextDecision, GuardUntil, ResonanceUntil, UnloadUntil, NextPassiveEffect, NextAudio, Clock;
    public bool BoutHostileHit, BoutCast;
    public MclslCareerCast Cast;
    public Dictionary<long,MclslCareerMark> Marks = new();
    public List<MclslCareerZone> Zones = new();
    public List<MclslCareerSummon> Summons = new();
}

