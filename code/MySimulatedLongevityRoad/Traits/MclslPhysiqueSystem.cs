using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Traits;

internal static class MclslPhysiqueSystem
{
    private sealed class Physique
    {
        internal readonly string Id, Name, Motto, Basis, Acquisition, Native, Mod;
        internal readonly string[] Stats;
        internal readonly int Annual, Study, Experience, WaterStudy, WaterExperience, FireStudy, FireExperience,
            Death, Explore, Stones, Breakthrough, FireDamage, CastExperience, Lifespan;
        internal readonly bool Congenital;
        internal Physique(string id, string name, string motto, string basis, string acquisition,
            string native, string mod, string[] stats, int Annual = 0, int Study = 0, int Experience = 0,
            int WaterStudy = 0, int WaterExperience = 0, int FireStudy = 0, int FireExperience = 0,
            int Death = 0, int Explore = 0, int Stones = 0, int Breakthrough = 0,
            int FireDamage = 0, int CastExperience = 0, int Lifespan = 0, bool Congenital = false)
        {
            Id = id; Name = name; Motto = motto; Basis = basis; Acquisition = acquisition;
            Native = native; Mod = mod; Stats = stats;
            this.Annual = Annual; this.Study = Study; this.Experience = Experience;
            this.WaterStudy = WaterStudy; this.WaterExperience = WaterExperience;
            this.FireStudy = FireStudy; this.FireExperience = FireExperience;
            this.Death = Death; this.Explore = Explore; this.Stones = Stones;
            this.Breakthrough = Breakthrough; this.FireDamage = FireDamage;
            this.CastExperience = CastExperience; this.Lifespan = Lifespan; this.Congenital = Congenital;
        }
    }

    private static readonly Physique[] Definitions =
    {
        new("MclslPhysiqueTreasure", "通灵宝体", "珍宝未现，灵觉先知。", "原著的通灵寻宝体质。", "先天稀有；编辑器可赋予。", "命中+120、闪避+2000、暴击率+40%", "秘境灵石奖励+50%", new[]{"accuracy:120","Dodge:2000","critical_chance:0.4"}, Stones:50, Congenital:true),
        new("MclslPhysiqueImmortal", "天生仙体", "无瘴而生，天生亲近道术。", "原著的天生仙体。", "先天稀有；编辑器可赋予。", "生命+4000、伤害+2200、生命倍率+6、伤害倍率+6", "年度真元+60%；全部法术研习+60%、升级经验+50%；出生不带仙凡瘴", new[]{"health:4000","damage:2200","multiplier_health:6","multiplier_damage:6"}, Annual:60, Study:60, Experience:50, Congenital:true),
        new("MclslPhysiqueWater", "水灵体", "临水纳灵，真气周流不息。", "原著的水灵体。", "先天稀有；编辑器可赋予。", "生命+3500、耐力+4000、生命倍率+6", "年度真元+35%；水属性法术研习+70%、升级经验+50%", new[]{"health:3500","stamina:4000","multiplier_health:6"}, Annual:35, WaterStudy:70, WaterExperience:50, Congenital:true),
        new("MclslPhysiqueWeak", "先天弱体", "先天精气不足，修行步步维艰。", "依据原著的先天孱弱体质设定，模组命名。", "先天稀有；编辑器可赋予。", "生命倍率−50%、耐力倍率−50%、有效寿限−50%", "年度真元获取−50%", new[]{"multiplier_health:-0.5","multiplier_stamina:-0.5","multiplier_lifespan:-0.5"}, Annual:-50, Congenital:true),
        new("MclslPhysiqueArray", "阵灵圣体", "身似天成阵眼，可感阵势流转。", "原著的阵道特殊体质，模组命名。", "先天稀有；编辑器可赋予。", "生命+4000、护甲+85、生命倍率+6", "秘境探索进度+50%、死亡率−15个百分点", new[]{"health:4000","armor:85","multiplier_health:6"}, Death:15, Explore:50, Congenital:true),
        new("MclslPhysiqueDao", "先天道体", "生而近道，术法易悟易成。", "原著的先天道体。", "先天稀有；编辑器可赋予。", "伤害+3000、伤害倍率+8、暴击率+60%", "年度真元+40%；全部法术研习及升级经验各+70%", new[]{"damage:3000","multiplier_damage:8","critical_chance:0.6"}, Annual:40, Study:70, Experience:70, Congenital:true),
        new("MclslPhysiqueEye", "天妒·破妄目", "一眼辨虚实，险境难藏形。", "依据原著天妒异目设定，模组命名。", "先天稀有；编辑器可赋予。", "命中+180、闪避+3000、暴击率+60%", "全部法术研习+50%；秘境死亡率−15个百分点", new[]{"accuracy:180","Dodge:3000","critical_chance:0.6"}, Study:50, Death:15, Congenital:true),
        new("MclslPhysiqueBone", "天妒·不朽骨", "异骨蕴生机，历险更易存身。", "依据原著天妒异骨设定，模组命名。", "先天稀有；编辑器可赋予。", "生命+6000、生命倍率+8、护甲+70、寿元+150", "秘境死亡率−12个百分点", new[]{"health:6000","multiplier_health:8","armor:70","lifespan:150"}, Death:12, Lifespan:150, Congenital:true),
        new("MclslPhysiqueFlame", "天妒·怒焰体", "怒意化炎，火术随心而盛。", "依据原著天妒怒焰体质设定，模组命名。", "先天稀有；编辑器可赋予。", "伤害+3000、伤害倍率+7、暴击率+60%", "年度真元+25%；火法研习+70%、升级经验+50%、伤害+40%", new[]{"damage:3000","multiplier_damage:7","critical_chance:0.6"}, Annual:25, FireStudy:70, FireExperience:50, FireDamage:40, Congenital:true),
        new("MclslPhysiqueBeastBlood", "上古妖血", "古血虽稀，肉身仍具异力。", "原著的上古妖族血脉。", "仅可核实血脉条件取得；编辑器可赋予。", "生命+5000、伤害+2000、护甲+85、生命倍率+7", "秘境死亡率−12个百分点、探索进度+25%", new[]{"health:5000","damage:2000","armor:85","multiplier_health:7"}, Death:12, Explore:25),
        new("MclslPhysiqueYinYang", "阴阳玉伴生体", "阴阳伴生，破境机缘更盛。", "依据原著阴阳玉伴生设定，模组命名。", "仅可核实伴生条件取得；编辑器可赋予。", "生命+4000、耐力+4000、生命倍率+6", "现有破境成功率+25个百分点", new[]{"health:4000","stamina:4000","multiplier_health:6"}, Breakthrough:25),
        new("MclslPhysiqueSymbiosis", "共修益元体", "气机相和，修行更易积累。", "依据原著共修增益体质设定，模组命名。", "仅可核实共修条件取得；编辑器可赋予。", "生命+3500、耐力+3500、生命倍率+6", "持有者年度真元+40%", new[]{"health:3500","stamina:3500","multiplier_health:6"}, Annual:40),
        new("MclslPhysiqueFurnace", "异兽融炉体", "异血熔身，坚韧如炉。", "依据原著异兽融血体质设定，模组命名。", "仅可核实融血条件取得；编辑器可赋予。", "生命+6000、伤害+2600、护甲+90、生命倍率+8", "秘境死亡率−15个百分点", new[]{"health:6000","damage:2600","armor:90","multiplier_health:8"}, Death:15),
        new("MclslPhysiqueNether", "幽族灵躯", "幽族气息深沉，神魂更稳。", "依据原著幽族血统设定，模组命名。", "仅可核实种族条件取得；编辑器可赋予。", "生命+3000、耐力+4000、抗性+512、生命倍率+5", "初次取得时现有心境+20，上限100", new[]{"health:3000","stamina:4000","resist:512","multiplier_health:5"}),
        new("MclslPhysiqueWar", "征伐圣体", "久历征伐，术法在实战中成熟。", "依据原著征伐淬体设定，模组命名。", "仅可核实征伐条件取得；编辑器可赋予。", "伤害+3000、伤害倍率+8、护甲+55、暴击率+60%", "已学法术的施放升级经验+70%", new[]{"damage:3000","multiplier_damage:8","armor:55","critical_chance:0.6"}, CastExperience:70),
        new("MclslPhysiqueForged", "万道锻体", "万道磨身，诸术触类旁通。", "依据原著万道磨炼肉身设定，模组命名。", "仅可核实锻体条件取得；编辑器可赋予。", "生命+5500、护甲+90、抗性+512、生命倍率+8", "年度真元+30%；全部法术研习+60%、升级经验+50%", new[]{"health:5500","armor:90","resist:512","multiplier_health:8"}, Annual:30, Study:60, Experience:50),
        new("MclslPhysiqueRenewal", "百折复元体", "屡经挫折，仍能积蓄生机。", "依据原著复元体质设定，模组命名。", "仅可核实复元条件取得；编辑器可赋予。", "生命+6500、寿元+200、生命倍率+8", "年度真元+30%、秘境死亡率−10个百分点", new[]{"health:6500","lifespan:200","multiplier_health:8"}, Annual:30, Death:10, Lifespan:200)
    };

    private static readonly Dictionary<string, Physique> ById = BuildIndex();
    private static readonly HashSet<string> LegendaryIds = new(StringComparer.Ordinal)
    {
        "MclslPhysiqueImmortal", "MclslPhysiqueDao", "MclslPhysiqueEye",
        "MclslPhysiqueBone", "MclslPhysiqueFlame", "MclslPhysiqueFurnace",
        "MclslPhysiqueWar", "MclslPhysiqueForged", "MclslPhysiqueRenewal"
    };
    // All 17 special physiques share a 20 / 10,000 (0.2%) birth chance.
    internal const int BirthChancePerTenThousand = 20;
    internal const int BirthWeightTotal = 25; // Nine legendary tickets and eight epic traits with two tickets each.
    private const string BirthCheckedKey = "mclsl.architecture.v2.actor.physique.birth_checked";
    private static Dictionary<string, Physique> BuildIndex()
    {
        var result = new Dictionary<string, Physique>(StringComparer.Ordinal);
        foreach (Physique entry in Definitions) result.Add(entry.Id, entry);
        return result;
    }

    internal static void Register()
    {
        MclslLocalizationBridge.RegisterKey("trait_group_" + MclslTraitRegistration.PhysiqueGroupId, "玄黄异禀");
        foreach (Physique entry in Definitions)
        {
            string rarity = LegendaryIds.Contains(entry.Id) ? "传奇" : "史诗";
            string birthProbability = IsLegendary(entry.Id) ? "0.008%" : "0.016%";
            string acquisition = "出生时自然抽取；编辑器可手动赋予。";
            MclslTraitRegistration.RegisterProfessionText(entry.Id, entry.Name, entry.Motto + "\n稀有度：" + rarity + "\n自然出生概率：" + birthProbability + "\n原著依据：" + entry.Basis + "\n原版属性：" + entry.Native + "\n模组效果：" + entry.Mod + "\n获取方式：" + acquisition);
            MclslTraitRegistration.RegisterTraitInfo(entry.Id, entry.Motto, "稀有度：" + rarity + "；自然出生概率：" + birthProbability + "；原著依据：" + entry.Basis + "；原版属性：" + entry.Native + "；模组效果：" + entry.Mod + "；获取方式：" + acquisition);
            MclslTraitRegistration.AddSpecial(entry.Id, "trait/" + entry.Id, true, true, 0f, 0f, 0f, 0f);
            ActorTrait trait = AssetManager.traits.get(entry.Id);
            if (trait == null) continue;
            trait.group_id = MclslTraitRegistration.PhysiqueGroupId;
            MclslTraitRegistration.SetPhysiqueRarity(trait, LegendaryIds.Contains(entry.Id));
            foreach (string pair in entry.Stats)
            {
                int split = pair.IndexOf(':');
                if (split <= 0 || !float.TryParse(pair.Substring(split + 1), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float amount)) continue;
                MclslTraitRegistration.SafeSetStat(trait.base_stats, pair.Substring(0, split), amount);
            }
        }
    }

    internal static bool IsPhysique(string id) => !string.IsNullOrWhiteSpace(id) && ById.ContainsKey(id);
    internal static IReadOnlyList<string> AllIds
    {
        get
        {
            string[] ids = new string[Definitions.Length];
            for (int i = 0; i < Definitions.Length; i++) ids[i] = Definitions[i].Id;
            return ids;
        }
    }
    internal static string DisplayName(string id) => id != null && ById.TryGetValue(id, out Physique entry)
        ? entry.Name : id ?? string.Empty;
    internal static bool IsLegendary(string id) => id != null && LegendaryIds.Contains(id);
    internal static string PickBirthPhysique(int chanceRoll, int weightedRoll)
    {
        if (chanceRoll < 0 || chanceRoll >= BirthChancePerTenThousand) return null;
        int ticket = ((weightedRoll % BirthWeightTotal) + BirthWeightTotal) % BirthWeightTotal;
        foreach (Physique entry in Definitions)
        {
            int weight = IsLegendary(entry.Id) ? 1 : 2;
            if (ticket < weight) return entry.Id;
            ticket -= weight;
        }
        return null;
    }
    internal static void MarkExistingAtLoad(Actor actor)
    {
        if (actor?.data != null) MclslActorAccessor.Set(actor, BirthCheckedKey, 1);
    }

    internal static void ReconcileOwner(Actor actor)
    {
        if (actor?.data == null) return;
        if (!HasAny(actor)) return;
        if (!MclslEligibility.TryGetSpecialPhysiqueEligibility(actor, out bool eligible)) return;
        bool removed = false;
        foreach (Physique entry in Definitions)
        {
            if (!actor.hasTrait(entry.Id)) continue;
            if (!eligible)
            {
                string id = entry.Id;
                MclslTraitGrantRouter.SuppressRouting(() => actor.removeTrait(id));
                removed = true;
                continue;
            }
            OnGranted(actor, entry.Id, existing: true);
        }
        if (removed) MclslRuntimeChanges.Publish(actor, MclslActorChange.Traits);
    }

    internal static bool HasAny(Actor actor)
    {
        if (actor?.data == null) return false;
        foreach (Physique entry in Definitions)
            if (actor.hasTrait(entry.Id)) return true;
        return false;
    }
    internal static void TryRollAtBirth(Actor actor)
    {
        if (actor?.data == null || MclslActorAccessor.GetInt(actor, BirthCheckedKey, 0) != 0) return;
        if (actor.getAge() > 1) { MarkExistingAtLoad(actor); return; }
        if (!MclslEligibility.TryGetSpecialPhysiqueEligibility(actor, out bool eligible)) return;
        MarkExistingAtLoad(actor);
        if (!eligible) return;
        foreach (Physique entry in Definitions)
            if (actor.hasTrait(entry.Id)) return;
        long id = MclslActorAccessor.Id(actor);
        ulong seed = unchecked((ulong)id * 11400714819323198485UL + 0x9E3779B97F4A7C15UL);
        seed ^= seed >> 30; seed *= 0xBF58476D1CE4E5B9UL;
        seed ^= seed >> 27; seed *= 0x94D049BB133111EBUL;
        seed ^= seed >> 31;
        string selected = PickBirthPhysique((int)(seed % 10000UL),
            (int)((seed / 10000UL) % (ulong)BirthWeightTotal));
        if (selected == null || !actor.addTrait(selected)) return;
        if (selected == "MclslPhysiqueImmortal") MclslActorAccessor.Set(actor, MclslActorDataKeys.MortalMiasma, 0);
    }
    private static int Sum(Actor actor, Func<Physique, int> selector, int maximum = int.MaxValue)
    {
        if (actor?.data == null) return 0;
        int sum = 0;
        foreach (Physique entry in Definitions)
            if (actor.hasTrait(entry.Id)) sum += selector(entry);
        return Math.Min(maximum, sum);
    }
    internal static int AnnualPercent(Actor actor) => Math.Clamp(Sum(actor, p => p.Annual), -50, 100);
    internal static int LifespanBonus(Actor actor) => Sum(actor, p => p.Lifespan);
    internal static int StudyPercent(Actor actor, string law) => Sum(actor, p => p.Study + (law == "水" ? p.WaterStudy : law == "火" ? p.FireStudy : 0), 100);
    internal static int ExperiencePercent(Actor actor, string law, bool cast) => Sum(actor, p => p.Experience + (law == "水" ? p.WaterExperience : law == "火" ? p.FireExperience : 0) + (cast ? p.CastExperience : 0), 100);
    internal static int DeathReduction(Actor actor) => Sum(actor, p => p.Death);
    internal static int ExplorePercent(Actor actor) => Sum(actor, p => p.Explore);
    internal static int StonesPercent(Actor actor) => Sum(actor, p => p.Stones);
    internal static int BreakthroughPoints(Actor actor) => Sum(actor, p => p.Breakthrough);
    internal static int FireDamagePercent(Actor actor) => Sum(actor, p => p.FireDamage);
    internal static void OnGranted(Actor actor, string id, bool existing = false)
    {
        if (actor?.data == null || !IsPhysique(id) || !MclslEligibility.CanOwnSpecialPhysique(actor)) return;
        if (!existing)
        {
            int aptitude = MclslAptitudeGiftCatalog.RollSpecialPhysiqueAptitude(
                MclslActorAccessor.Id(actor) + "|" + id, id is "MclslPhysiqueImmortal" or "MclslPhysiqueDao");
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, aptitude);
        }
        if (!MclslSpiritualRootSystem.EnsureForSpecialPhysique(actor)) return;
        MclslTraitRegistration.SyncGiftTrait(actor,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50),
            allowEntry: actor.getAge() >= 6f);
        if (id != "MclslPhysiqueNether" || actor?.data == null) return;
        const string key = "mclsl.architecture.v2.actor.physique.nether_mind_granted";
        if (MclslActorAccessor.GetInt(actor, key, 0) != 0) return;
        int current = MclslMindSystem.EnsureMindState(actor);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, Math.Min(100, current + 20));
        MclslActorAccessor.Set(actor, key, 1);
    }
}
