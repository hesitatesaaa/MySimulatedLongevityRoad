using System;
using MySimulatedLongevityRoad.Data;

namespace EventArchiveTests;

public static class Program
{
    static int Checks;
    static void Check(bool ok, string name)
    {
        if (!ok) throw new Exception(name);
        Checks++;
    }

    // Small, dependency-free model of the trim decision the repository delegates
    // to MclslEventTrimPolicy. We exercise the policy directly (the same code path
    // the game uses), keeping caps representative of the repository constants.
    const int MaxOrdinary = 400;
    const int MaxMilestones = 128;

    static int Hysteresis(int cap) => MclslEventTrimPolicy.Hysteresis(cap);

    public static void Main()
    {
        // --- Non-forced (lazy) mode: skips the scan inside the hysteresis zone ---
        // A steady stream sitting *at* the caps must not trigger a trim.
        Check(!MclslEventTrimPolicy.NeedsTrim(MaxOrdinary, MaxMilestones, MaxOrdinary, MaxMilestones, force: false),
            "at-cap stream: no lazy trim");
        // Below both caps is never trimmed.
        Check(!MclslEventTrimPolicy.NeedsTrim(MaxOrdinary - 1, MaxMilestones - 1, MaxOrdinary, MaxMilestones, force: false),
            "below caps: no lazy trim");
        // Empty archive never trims.
        Check(!MclslEventTrimPolicy.NeedsTrim(0, 0, MaxOrdinary, MaxMilestones, force: false),
            "empty archive: no trim");
        // Overshooting only the milestone cap within ordinary hysteresis still trims.
        Check(MclslEventTrimPolicy.NeedsTrim(MaxOrdinary, Hysteresis(MaxMilestones) + 1, MaxOrdinary, MaxMilestones, force: false),
            "milestone overshoot in lazy mode trims");
        // Overshooting the ordinary cap beyond hysteresis trims.
        Check(MclslEventTrimPolicy.NeedsTrim(Hysteresis(MaxOrdinary) + 1, MaxMilestones, MaxOrdinary, MaxMilestones, force: false),
            "ordinary overshoot in lazy mode trims");
        // Inside hysteresis of both caps: no scan (the whole point).
        Check(!MclslEventTrimPolicy.NeedsTrim(Hysteresis(MaxOrdinary), Hysteresis(MaxMilestones), MaxOrdinary, MaxMilestones, force: false),
            "both inside hysteresis: no trim");
        // Exactly the overshoot boundary should not trim (<=), just past it trims.
        Check(!MclslEventTrimPolicy.NeedsTrim(Hysteresis(MaxOrdinary), MaxMilestones, MaxOrdinary, MaxMilestones, force: false),
            "at ordinary hysteresis edge: no trim");
        Check(MclslEventTrimPolicy.NeedsTrim(Hysteresis(MaxOrdinary) + 1, MaxMilestones, MaxOrdinary, MaxMilestones, force: false),
            "just past ordinary hysteresis edge: trim");

        // --- Forced mode (save boundary): caps enforced unconditionally ---
        // At exactly the caps -> nothing to remove.
        Check(!MclslEventTrimPolicy.NeedsTrim(MaxOrdinary, MaxMilestones, MaxOrdinary, MaxMilestones, force: true),
            "forced at caps: no trim");
        // Below caps in forced mode too.
        Check(!MclslEventTrimPolicy.NeedsTrim(MaxOrdinary - 5, MaxMilestones - 5, MaxOrdinary, MaxMilestones, force: true),
            "forced below caps: no trim");
        // Past the cap in forced mode whether or not hysteresis is reached -> trims.
        Check(MclslEventTrimPolicy.NeedsTrim(Hysteresis(MaxOrdinary) + 1, MaxMilestones, MaxOrdinary, MaxMilestones, force: true),
            "forced past ordinary cap: trim (no hysteresis exemption)");
        Check(MclslEventTrimPolicy.NeedsTrim(MaxOrdinary, MaxMilestones + 1, MaxOrdinary, MaxMilestones, force: true),
            "forced past milestone cap: trim");
        // Forced mode has no hysteresis exemption: even one event over the cap trims.
        Check(MclslEventTrimPolicy.NeedsTrim(MaxOrdinary + 1, MaxMilestones, MaxOrdinary, MaxMilestones, force: true)
                && MclslEventTrimPolicy.RemoveOrdinary(MaxOrdinary + 1, MaxOrdinary) == 1,
            "forced just-over ordinary cap trims and removes exactly one");

        // --- Removal budget helpers ---
        Check(MclslEventTrimPolicy.RemoveOrdinary(MaxOrdinary, MaxOrdinary) == 0, "removeOrdinary at cap is zero");
        Check(MclslEventTrimPolicy.RemoveOrdinary(MaxOrdinary + 20, MaxOrdinary) == 20, "removeOrdinary positive");
        Check(MclslEventTrimPolicy.RemoveOrdinary(10, MaxOrdinary) == 0, "removeOrdinary below cap is zero");
        Check(MclslEventTrimPolicy.RemoveMilestones(MaxMilestones + 7, MaxMilestones) == 7, "removeMilestones positive");
        Check(MclslEventTrimPolicy.RemoveMilestones(MaxMilestones, MaxMilestones) == 0, "removeMilestones at cap is zero");

        // --- Hysteresis math ---
        Check(MclslEventTrimPolicy.Hysteresis(400) == 500, "1.25x of 400 is 500");
        Check(MclslEventTrimPolicy.Hysteresis(128) == 160, "1.25x of 128 is 160");
        Check(MclslEventTrimPolicy.Hysteresis(1) == 2, "1.25x of 1 rounds up to 2");

        Console.WriteLine("PASS event archive trim policy: " + Checks + " checks");
    }
}