using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;

internal static class PhysiqueLifespanTests
{
    internal static void Run()
    {
        MclslTraitRegistration.ProfessionTexts.Clear();
        MclslPhysiqueSystem.Register();

        var weakStats = AssetManager.traits.get("MclslPhysiqueWeak")!.base_stats.Values;
        Check(weakStats.GetValueOrDefault("multiplier_health") == -0.5f,
            "weak physique must reduce health multiplier by 50 percent");
        Check(weakStats.GetValueOrDefault("multiplier_stamina") == -0.5f,
            "weak physique must reduce stamina multiplier by 50 percent");
        Check(weakStats.GetValueOrDefault("multiplier_lifespan") == -0.5f,
            "weak physique must reduce lifespan multiplier by 50 percent");
        Check(!weakStats.ContainsKey("health") && !weakStats.ContainsKey("stamina") && !weakStats.ContainsKey("lifespan"),
            "weak physique must not retain fixed stat penalties");
        Check(MclslTraitRegistration.ProfessionTexts["MclslPhysiqueWeak"].Contains("有效寿限−50%")
            && MclslTraitRegistration.ProfessionTexts["MclslPhysiqueWeak"].Contains("年度真元获取−50%"),
            "weak physique description must show fixed percentage penalties");

        Check(AssetManager.traits.get("MclslPhysiqueBone")!.base_stats.Values.GetValueOrDefault("lifespan") == 150f,
            "immortal bone must add 150 years");
        Check(AssetManager.traits.get("MclslPhysiqueRenewal")!.base_stats.Values.GetValueOrDefault("lifespan") == 200f,
            "renewal physique must add 200 years");

        var weak = ActorAt(MclslRealmIds.LianQi, "MclslPhysiqueWeak");
        Check(MclslLongevityRules.ExpectedLifespan(weak, weak.Realm) == 140,
            "weak physique must halve the Lianqi lifespan once");
        weak.stats["lifespan"] = 0f;
        MclslLongevityRules.ApplyRuntimeLifespan(weak);
        Check(weak.stats["lifespan"] == 140f,
            "runtime lifespan repair must use the halved effective lifespan");
        weak.stats["lifespan"] = 280f;
        MclslLongevityRules.ApplyRuntimeLifespan(weak);
        Check(weak.stats["lifespan"] == 140f,
            "runtime lifespan repair must lower an unhalved realm lifespan");
        Check(MclslLongevityRules.ExpectedLifespan(ActorAt(MclslRealmIds.HuaShen, "MclslPhysiqueWeak"), MclslRealmIds.HuaShen) == 1040,
            "weak physique must halve the Huashen lifespan");
        Check(MclslLongevityRules.ExpectedLifespan(ActorAt(MclslRealmIds.HeDao, "MclslPhysiqueWeak"), MclslRealmIds.HeDao) == 1540,
            "weak physique must halve the Hedao lifespan");

        var weakWithBone = ActorAt(MclslRealmIds.LianQi, "MclslPhysiqueWeak", "MclslPhysiqueBone");
        Check(MclslLongevityRules.ExpectedLifespan(weakWithBone, weakWithBone.Realm) == 215,
            "weak physique must halve realm lifespan together with the 150-year physique bonus");
        var weakWithRenewal = ActorAt(MclslRealmIds.LianQi, "MclslPhysiqueWeak", "MclslPhysiqueRenewal");
        Check(MclslLongevityRules.ExpectedLifespan(weakWithRenewal, weakWithRenewal.Realm) == 240,
            "weak physique must halve realm lifespan together with the 200-year physique bonus");

        var weakReceiver = ActorAt(MclslRealmIds.JinDan, "MclslPhysiqueWeak");
        var donor = ActorAt(MclslRealmIds.LianQi);
        donor.Age = 50;
        Check(MclslLongevityRules.TryDrainYears(weakReceiver, donor, 40, out int received) && received == 32,
            "lifespan transfer must still settle the normal receiver share");
        Check(MclslLongevityRules.ExpectedLifespan(weakReceiver, weakReceiver.Realm) == 356,
            "weak receiver must apply its 50 percent penalty after transferred years");
        Check(MclslLongevityRules.ExpectedLifespan(donor, donor.Realm) == 240,
            "lifespan donor penalty must remain reflected in the effective limit");

        var minimum = ActorAt(MclslRealmIds.LianQi, "MclslPhysiqueWeak");
        minimum.Values["lifespan_drained_penalty"] = 10000;
        Check(MclslLongevityRules.ExpectedLifespan(minimum, minimum.Realm) == 20,
            "weak physique must preserve the 20-year lifespan floor");
        minimum.stats["lifespan"] = 140f;
        MclslLongevityRules.ApplyRuntimeLifespan(minimum);
        Check(minimum.stats["lifespan"] == 20f,
            "runtime lifespan repair must preserve the 20-year floor for weak physiques");

        var weakAnnual = ActorAt(MclslRealmIds.LianQi, "MclslPhysiqueWeak");
        Check(MclslPhysiqueSystem.AnnualPercent(weakAnnual) == -50,
            "weak physique must reduce annual true essence gain by 50 percent");
        weakAnnual.addTrait("MclslPhysiqueImmortal");
        Check(MclslPhysiqueSystem.AnnualPercent(weakAnnual) == 10,
            "annual physique bonuses must stack additively with weak physique");

        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
    }

    private static Actor ActorAt(string realm, params string[] traits)
    {
        var actor = new Actor { Realm = realm };
        foreach (string trait in traits) actor.addTrait(trait);
        return actor;
    }
}
