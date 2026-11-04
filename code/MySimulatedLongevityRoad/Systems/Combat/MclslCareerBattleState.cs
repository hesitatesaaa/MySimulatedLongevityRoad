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
    public double Pulse;
    // 灵契师单契灵：Bound 表示终身绑定；NextAttack / NextHeal 为隼、鹿的节拍。
    public bool Bound;
    public double NextAttack, NextHeal;
}
internal sealed class MclslCareerMark
{
    public int SwordHits, Sigils;
    public double Expires, OwnerHit, SpiritHit, NextAssist;
    // 契印击施加的 4 秒契印到期时刻（契灵攻击该目标增伤）。
    public double ContractMark;
}
internal sealed class MclslCareerBattleState
{
    public int Version=2;
    public long Owner;
    public int Career, Sword, Rage, Elements, LastElement;
    public bool Counter, FullCycle;
    public double LastAction, NextDecision, GuardUntil, ResonanceUntil, UnloadUntil, NextPassiveEffect, NextAudio, Clock;
    // 灵息灌注到期时刻。
    public double PetBuffUntil;
    public bool BoutHostileHit, BoutCast;
    public MclslCareerCast Cast;
    public Dictionary<long,MclslCareerMark> Marks = new();
    public List<MclslCareerZone> Zones = new();
    public List<MclslCareerSummon> Summons = new();
}
