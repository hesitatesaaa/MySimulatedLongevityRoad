using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslGeneratedObjectFactory
{
    internal const string FoundationHuman = "human";
    internal const string FoundationEarth = "earth";
    internal const string FoundationHeaven = "heaven";

    internal static MclslGeneratedItemRecord CreateFoundationWonder(Actor actor, int year, int quality, IReadOnlyList<string> lawTags)
    {
        string origin = ActorOrigin(actor, year, "奇遇");
        int seed = StableHash(MclslWorldRunRepository.Current.RunId + "|foundation|" + MclslActorAccessor.Id(actor) + "|" + year);
        MclslGeneratedItemRecord item = CreateItem(MclslGeneratedKinds.FoundationWonder, year, quality, lawTags, origin, seed);
        ApplyFoundationClassification(item, seed);
        item.Origin = ActorOrigin(actor, year, FoundationOriginReason(item.Category));
        item.HolderActorId = MclslActorAccessor.Id(actor);
        item.HolderName = SafeDisplayName(actor);
        item.Consumed = true;
        MclslWorldRunRepository.RegisterGeneratedItem(item);
        return item;
    }

    internal static MclslGeneratedItemRecord CreateHeavenEarthEssence(Actor actor, MclslWorldCaveRecord cave, int year, int compatibility)
    {
        string origin = "自“" + cave.Name + "”中夺取并炼化";
        string[] tags = SplitTags(cave.LawTags);
        int quality = Math.Clamp(cave.Quality + (compatibility >= 80 ? 1 : 0), 1, 4);
        int seed = StableHash(cave.Id + "|essence|" + cave.RefinedCount + "|" + MclslActorAccessor.Id(actor) + "|" + year);
        MclslGeneratedItemRecord item = CreateItem(MclslGeneratedKinds.HeavenEarthEssence, year, quality, tags, origin, seed);
        item.HolderActorId = MclslActorAccessor.Id(actor);
        item.HolderName = SafeDisplayName(actor);
        item.Consumed = true;
        item.SourceObjectId = cave.Id;
        item.Description = "从“" + cave.Name + "”洞天根源中剥离的一缕天地之精，蕴含“" + string.Join("、", tags) + "”法则，已被炼入元婴洞天。";
        MclslWorldRunRepository.RegisterGeneratedItem(item);
        return item;
    }

    internal static MclslGeneratedItemRecord CreateHeavenEarthMarrow(Actor actor, MclslWorldChangeRecord change, int year, int compatibility)
    {
        string origin = "自天地之变“" + change.Name + "”中抽取并炼化";
        string[] tags = SplitTags(change.LawTags);
        int quality = Math.Clamp(change.Quality + (compatibility >= 85 ? 1 : 0), 1, 4);
        int seed = StableHash(change.Id + "|marrow|" + change.ExtractedCount + "|" + MclslActorAccessor.Id(actor) + "|" + year);
        MclslGeneratedItemRecord item = CreateItem(MclslGeneratedKinds.WorldChangeMarrow, year, quality, tags, origin, seed);
        item.HolderActorId = MclslActorAccessor.Id(actor);
        item.HolderName = SafeDisplayName(actor);
        item.Consumed = true;
        item.SourceObjectId = change.Id;
        item.Description = "从天地之变“" + change.Name + "”中抽出的一缕天地之髓，凝聚“" + string.Join("、", tags) + "”之变，已被炼入化神根基。";
        MclslWorldRunRepository.RegisterGeneratedItem(item);
        return item;
    }

    internal static MclslWorldChangeRecord CreateWorldChange(int year, int sequence, string location, string kingdom, IReadOnlyList<string> lawTags, int quality, string origin, string sourceType, int mapX = -1, int mapY = -1)
    {
        int seed = StableHash(MclslWorldRunRepository.Current.RunId + "|world_change|" + sequence + "|" + year + "|" + location + "|" + sourceType);
        string[] tags = NormalizeTags(lawTags);
        string name = GenerateUniqueName(MclslGeneratedKinds.WorldChange, tags, seed, origin);
        int intensity = Math.Clamp(58 + Math.Clamp(quality, 1, 4) * 10 + PositiveHash(seed + "|intensity") % 9, 60, 100);
        string terrain = MclslNativeTerrainProfileCatalog.ForTags(tags, sourceType).Summary;
        int marrowCapacity = ResourceUseCapacity();
        return new MclslWorldChangeRecord
        {
            Id = "change_" + PositiveHash(seed + "|id").ToString("x8") + "_" + sequence,
            Name = name,
            Description = "天地在" + (string.IsNullOrWhiteSpace(location) ? "无主荒域" : location) + "发生剧变，‘" + string.Join("、", tags) + "’法则彼此冲荡，形成可供化神抽髓的天地之变。",
            Origin = string.IsNullOrWhiteSpace(origin) ? "天地自然演化" : origin,
            LawTags = string.Join(",", tags),
            Quality = Math.Clamp(quality, 1, 4),
            StartYear = Math.Max(0, year),
            MarrowCapacity = marrowCapacity,
            RemainingMarrow = marrowCapacity,
            Intensity = intensity,
            State = "活跃",
            LocationName = string.IsNullOrWhiteSpace(location) ? "天地之间" : location,
            NativeKingdomName = string.IsNullOrWhiteSpace(kingdom) ? "无主" : kingdom,
            MapX = mapX,
            MapY = mapY,
            SourceType = string.IsNullOrWhiteSpace(sourceType) ? "world_event" : sourceType,
            NativeTerrainEffect = terrain
        };
    }

    internal static MclslSectRuinRecord CreateSectRuin(int year, int sequence, string location, string kingdom, IReadOnlyList<string> lawTags, int quality, string category)
    {
        int seed = StableHash(MclslWorldRunRepository.Current.RunId + "|sect_ruin|" + sequence + "|" + year + "|" + location + "|" + category);
        string[] tags = NormalizeTags(lawTags);
        string name = GenerateUniqueName(MclslGeneratedKinds.SectRuin, tags, seed, category + "|" + location);
        int dangerBase = category.Contains("秘境") ? 55 : category.Contains("遗府") || category.Contains("洞府") || category.Contains("遗藏") || category.Contains("旧府") ? 48 : 25;
        int danger = Math.Clamp(dangerBase + quality * 8 + PositiveHash(seed + "|danger") % 18, 20, 95);
        int depth = Math.Clamp(3 + quality * 2 + PositiveHash(seed + "|depth") % 4, 3, 12);
        int value = Math.Clamp(2 + quality * 2 + PositiveHash(seed + "|value") % 4, 3, 12);
        return new MclslSectRuinRecord
        {
            Id = "ruin_" + PositiveHash(seed + "|id").ToString("x8") + "_" + sequence,
            Name = name,
            Description = category + "，残存“" + string.Join("、", tags) + "”法则与旧日禁制。",
            Category = category,
            LawTags = string.Join(",", tags),
            Quality = Math.Clamp(quality, 1, 4),
            Danger = danger,
            BornYear = Math.Max(0, year),
            LocationName = string.IsNullOrWhiteSpace(location) ? "无主荒域" : location,
            NativeKingdomName = string.IsNullOrWhiteSpace(kingdom) ? "无主" : kingdom,
            Depth = depth,
            RemainingValue = value,
            State = "显世"
        };
    }

    internal static MclslWorldCaveRecord CreateWorldCave(int year, int sequence, string location, string kingdom, IReadOnlyList<string> lawTags, int quality, int mapX = -1, int mapY = -1)
    {
        int seed = StableHash(MclslWorldRunRepository.Current.RunId + "|cave|" + sequence + "|" + year + "|" + location);
        string[] tags = NormalizeTags(lawTags);
        string name = GenerateUniqueName(MclslGeneratedKinds.WorldCave, tags, seed, location);
        int integrity = Math.Clamp(58 + Math.Clamp(quality, 1, 4) * 9 + PositiveHash(seed + "|integrity") % 10, 60, 100);
        int essenceCapacity = ResourceUseCapacity();
        return new MclslWorldCaveRecord
        {
            Id = "cave_" + PositiveHash(seed + "|id").ToString("x8") + "_" + sequence,
            Name = name,
            Description = "于" + (string.IsNullOrWhiteSpace(location) ? "无主荒域" : location) + "显化，由“" + string.Join("、", tags) + "”法则长期交汇而成。",
            LawTags = string.Join(",", tags),
            Quality = Math.Clamp(quality, 1, 4),
            BornYear = Math.Max(0, year),
            LocationName = string.IsNullOrWhiteSpace(location) ? "无主荒域" : location,
            NativeKingdomName = string.IsNullOrWhiteSpace(kingdom) ? "无主" : kingdom,
            MapX = mapX,
            MapY = mapY,
            EssenceCapacity = essenceCapacity,
            RemainingEssence = essenceCapacity,
            Integrity = integrity,
            State = "活跃"
        };
    }

    private static int ResourceUseCapacity()
    {
        return MclslInverseTruthSystem.IsTruthReversed("truth_player_trace_persistence") ? 2 : 1;
    }

    internal static bool MigrateLegacyFoundationWonder(Actor actor, int year)
    {
        if (!MclslActorAccessor.IsCultivator(actor)) return false;
        string id = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderId, string.Empty);
        string name = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderName, string.Empty);
        if (string.IsNullOrWhiteSpace(name)) return false;
        bool legacy = id.StartsWith("wonder_", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderDescription, string.Empty));
        if (!legacy) return false;
        string[] tags = SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderTags, "灵"));
        int quality = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderQuality, 1), 1, 4);
        int seed = StableHash(MclslWorldRunRepository.Current.RunId + "|migrate_foundation|" + MclslActorAccessor.Id(actor));
        MclslGeneratedItemRecord item = CreateItem(MclslGeneratedKinds.FoundationWonder, year, quality, tags, ActorOrigin(actor, year, "旧世奇物重定名"), seed);
        ApplyFoundationClassification(item, seed);
        item.HolderActorId = MclslActorAccessor.Id(actor);
        item.HolderName = MclslActorAccessor.DisplayName(actor);
        item.Consumed = true;
        MclslWorldRunRepository.RegisterGeneratedItem(item);
        ApplyFoundation(actor, item);
        return true;
    }

    internal static void ApplyFoundation(Actor actor, MclslGeneratedItemRecord item)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderId, item.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderName, item.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderQuality, item.Quality);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderTags, item.LawTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderCategory, FoundationCategory(item));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderGrade, item.Grade ?? string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderCompleteness, item.Completeness);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderRuleStrength, item.RuleStrength);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderDescription, item.Description);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderOrigin, item.Origin);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderEffects, item.AttributeText);
    }

    private static MclslGeneratedItemRecord CreateItem(string kind, int year, int quality, IReadOnlyList<string> lawTags, string origin, int seed)
    {
        string[] tags = NormalizeTags(lawTags);
        string name = GenerateUniqueName(kind, tags, seed, origin);
        string typeName = kind switch
        {
            MclslGeneratedKinds.HeavenEarthEssence => "天地之精",
            MclslGeneratedKinds.WorldChangeMarrow => "天地之髓",
            _ => "筑基奇物"
        };
        return new MclslGeneratedItemRecord
        {
            Id = kind + "_" + PositiveHash(seed + "|" + name).ToString("x8") + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Kind = kind,
            Name = name,
            Description = "此物为" + typeName + "，蕴含“" + string.Join("、", tags) + "”之性。" + DescriptionTail(kind, quality),
            Origin = origin ?? string.Empty,
            LawTags = string.Join(",", tags),
            AttributeText = RootText(kind, tags, quality),
            Quality = Math.Clamp(quality, 1, 4),
            CreatedYear = Math.Max(0, year)
        };
    }

    internal static string FoundationCategory(MclslGeneratedItemRecord item)
    {
        if (item == null) return FoundationHuman;
        if (item.Kind != MclslGeneratedKinds.FoundationWonder) return string.Empty;
        return string.IsNullOrWhiteSpace(item.Category) ? FoundationHuman : item.Category;
    }

    internal static string FoundationRankText(MclslGeneratedItemRecord item)
    {
        if (item == null) return "人之奇";
        return FoundationRankText(FoundationCategory(item), item.Grade, item.Completeness, item.RuleStrength, item.Quality);
    }

    internal static string FoundationRankText(string category, string grade, int completeness, int ruleStrength, int quality)
    {
        category = string.IsNullOrWhiteSpace(category) ? FoundationHuman : category;
        if (category == FoundationHeaven) return "天之奇";
        if (category == FoundationEarth) return completeness > 0 ? "地之奇 完整度" + completeness + "%" : "地之奇";
        string resolvedGrade = string.IsNullOrWhiteSpace(grade) ? HumanGrade(quality) : grade;
        return "人之奇·" + resolvedGrade;
    }

    internal static string FoundationRankColor(string category)
    {
        category = string.IsNullOrWhiteSpace(category) ? FoundationHuman : category;
        return category switch
        {
            FoundationHeaven => "#FF5A5A",
            FoundationEarth => "#9CD7FF",
            _ => "#CFC7B2"
        };
    }

    private static void ApplyFoundationClassification(MclslGeneratedItemRecord item, int seed)
    {
        if (item == null || item.Kind != MclslGeneratedKinds.FoundationWonder) return;
        string category = DetermineFoundationCategory(seed);
        item.Category = category;
        if (category == FoundationHeaven)
        {
            item.Quality = 4;
            item.Grade = string.Empty;
            item.Completeness = Math.Clamp(82 + PositiveHash(seed + "|complete") % 15, 80, 96);
            item.RuleStrength = Math.Clamp(88 + PositiveHash(seed + "|rule") % 12, 88, 99);
        }
        else if (category == FoundationEarth)
        {
            item.Quality = Math.Clamp(Math.Max(3, item.Quality), 3, 4);
            item.Grade = string.Empty;
            item.Completeness = Math.Clamp(62 + item.Quality * 6 + PositiveHash(seed + "|complete") % 17, 60, 94);
            item.RuleStrength = Math.Clamp(58 + item.Quality * 7 + PositiveHash(seed + "|rule") % 18, 58, 92);
        }
        else
        {
            item.Quality = Math.Clamp(item.Quality, 1, 3);
            item.Grade = HumanGrade(item.Quality);
            item.Completeness = Math.Clamp(42 + item.Quality * 12 + PositiveHash(seed + "|complete") % 16, 40, 84);
            item.RuleStrength = Math.Clamp(34 + item.Quality * 11 + PositiveHash(seed + "|rule") % 14, 30, 78);
        }
        item.Description = FoundationDescription(item);
        item.AttributeText = FoundationRootText(item);
    }

    private static string DetermineFoundationCategory(int seed)
    {
        int roll = PositiveHash(seed + "|category") % 1000;
        int existingHeaven = MclslWorldRunRepository.Current?.GeneratedItems?.Count(x => x != null && x.Kind == MclslGeneratedKinds.FoundationWonder && x.Category == FoundationHeaven) ?? 0;
        if (existingHeaven < 3 && roll < 8) return FoundationHeaven;
        if (roll < 120) return FoundationEarth;
        return FoundationHuman;
    }

    private static string HumanGrade(int quality) => quality switch { >= 3 => "上品", 2 => "中品", _ => "下品" };

    private static string FoundationOriginReason(string category) => category switch
    {
        FoundationHeaven => "天之奇垂迹",
        FoundationEarth => "地之奇现世",
        _ => "人之奇感应"
    };

    private static string FoundationDescription(MclslGeneratedItemRecord item)
    {
        string tags = string.Join("、", SplitTags(item.LawTags));
        if (FoundationCategory(item) == FoundationHeaven)
            return item.Name + "为天之奇，暗合玄黄高层天道，承载“" + tags + "”之理。得之者仍需逐步悟解，方能以此支撑后续大道。";
        if (FoundationCategory(item) == FoundationEarth)
            return item.Name + "为地之奇，由山河异变与天地奇景孕生，蕴含“" + tags + "”法则，完整度" + item.Completeness + "%。";
        return item.Name + "为人之奇·" + item.Grade + "，由前代修士陨落后经天地转化而成，残留“" + tags + "”道意。";
    }

    private static string FoundationRootText(MclslGeneratedItemRecord item)
    {
        string tags = string.Join("、", SplitTags(item.LawTags));
        if (FoundationCategory(item) == FoundationHeaven)
            return "类别：天之奇；法则：" + tags + "；规则强度：" + item.RuleStrength + "%；后续可支撑更高大道，但悟解难度极高。";
        if (FoundationCategory(item) == FoundationEarth)
            return "类别：地之奇；法则：" + tags + "；完整度：" + item.Completeness + "%；规则强度：" + item.RuleStrength + "%；结丹法池更宽，炼化更重悟性。";
        return "类别：人之奇；品阶：" + item.Grade + "；法则：" + tags + "；含前人道意，筑基较易，根基受品阶影响。";
    }

    internal static string GenerateUniqueName(string kind, string[] tags, int seed, string source)
    {
        string[] forms = MclslProceduralLexicon.Forms(kind);
        string primary = tags.Length > 0 ? tags[0] : "灵";
        string secondary = tags.Length > 1 ? tags[1] : primary;
        string[] primaryRoots = MclslProceduralLexicon.Roots(primary);
        string[] secondaryRoots = MclslProceduralLexicon.Roots(secondary);
        for (int attempt = 0; attempt < 256; attempt++)
        {
            int hash = PositiveHash(seed + "|" + kind + "|" + attempt + "|" + source);
            string prefix = Pick(MclslProceduralLexicon.Prefixes, hash);
            string nature = Pick(MclslProceduralLexicon.NatureWords, hash / 7);
            string root1 = Pick(primaryRoots, hash / 13);
            string root2 = Pick(secondaryRoots, hash / 23);
            if (root2 == root1 && secondaryRoots.Length > 1) root2 = secondaryRoots[((hash / 23) + 1) % secondaryRoots.Length];
            string form = Pick(forms, hash / 31);
            string candidate = (hash % 8) switch
            {
                0 => prefix + root1 + form,
                1 => root1 + root2 + form,
                2 => nature + root1 + form,
                3 => prefix + nature + form,
                4 => root1 + nature + form,
                5 => prefix + root1 + root2 + form,
                6 => nature + root1 + root2 + form,
                _ => root2 + prefix + form
            };
            candidate = Collapse(candidate);
            if (MclslWorldRunRepository.TryReserveGeneratedName(candidate)) return candidate;
        }
        string fallbackBase = MclslProceduralLexicon.Prefixes[PositiveHash(seed + "|prefix") % MclslProceduralLexicon.Prefixes.Length]
            + MclslProceduralLexicon.Roots(primary)[PositiveHash(seed + "|root") % MclslProceduralLexicon.Roots(primary).Length]
            + forms[PositiveHash(seed + "|form") % forms.Length];
        for (int i = 0; i < 60; i++)
        {
            string fallback = fallbackBase + "·" + StemBranch(PositiveHash(seed + "|fallback|" + i));
            if (MclslWorldRunRepository.TryReserveGeneratedName(fallback)) return fallback;
        }
        while (true)
        {
            int sequence = MclslWorldRunRepository.NextProceduralSequence();
            string fallback = fallbackBase + "·孤本" + ChineseNumber(sequence);
            if (MclslWorldRunRepository.TryReserveGeneratedName(fallback)) return fallback;
        }
    }

    internal static void NormalizeActorRootText(Actor actor)
    {
        if (actor?.data == null) return;
        if (NeedsRootTextMigration(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderEffects, string.Empty)))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderEffects, RootText(MclslGeneratedKinds.FoundationWonder, SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderTags, string.Empty)), MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderQuality, 1)));
        if (NeedsRootTextMigration(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceEffects, string.Empty)))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceEffects, RootText(MclslGeneratedKinds.HeavenEarthEssence, SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags, string.Empty)), MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentEssenceQuality, 1)));
        if (NeedsRootTextMigration(MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowEffects, string.Empty)))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowEffects, RootText(MclslGeneratedKinds.WorldChangeMarrow, SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowTags, string.Empty)), MclslActorAccessor.GetInt(actor, MclslActorDataKeys.DivineMarrowQuality, 1)));
    }

    internal static string RootText(string kind, IReadOnlyList<string> tags, int quality)
    {
        string tagText = string.Join("、", tags);
        string qualityText = quality switch { 4 => "玄奇", 3 => "天奇", 2 => "地奇", _ => "凡奇" };
        if (kind == MclslGeneratedKinds.FoundationWonder)
        {
            return "筑基奇物：" + tagText + "；实际参与：结丹法则池、金丹法数、纯度、协调与后续适配。";
        }
        if (kind == MclslGeneratedKinds.HeavenEarthEssence)
        {
            return "元婴根基：" + tagText + "；精粹品质：" + qualityText + "；实际参与：化神抽髓适配、抽髓争夺强度、后续祭魄适配。";
        }
        if (kind == MclslGeneratedKinds.WorldChangeMarrow)
        {
            return "化神根基：" + tagText + "；髓质品质：" + qualityText + "；实际参与：祭魄适配、合道稳定、逆理方向。";
        }
        return "法则根基：" + tagText + "；品质：" + qualityText + "。";
    }

    private static bool NeedsRootTextMigration(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        return value.Contains("攻击速度") || value.Contains("移动速度") || value.Contains("爆发威能") || value.Contains("后续法则权重") || value.Contains("术法威能");
    }

    private static string DescriptionTail(string kind, int quality)
    {
        string qualityText = quality switch { 4 => "其性近乎玄奇，极难再现。", 3 => "其性天成，根基深厚。", 2 => "其性地蕴，可堪大用。", _ => "其性尚浅，却足以承载新法。" };
        if (kind == MclslGeneratedKinds.HeavenEarthEssence) return "炼化后可夺天地之精，以成元婴。" + qualityText;
        if (kind == MclslGeneratedKinds.WorldChangeMarrow) return "炼化后可抽天地之髓，以得化神。" + qualityText;
        return "用于假天地之奇，以筑道基。" + qualityText;
    }

    private static string ActorOrigin(Actor actor, int year, string reason)
    {
        string city = string.IsNullOrWhiteSpace(actor?.city?.data?.name) ? "无名荒野" : actor.city.data.name;
        string kingdom = string.IsNullOrWhiteSpace(actor?.kingdom?.data?.name) ? "无国之地" : actor.kingdom.data.name;
        return year + "年，" + SafeDisplayName(actor) + "于" + kingdom + "·" + city + "因" + reason + "所得";
    }

    private static string SafeDisplayName(Actor actor)
    {
        try { return actor?.data == null ? "某修士" : MclslActorAccessor.DisplayName(actor); }
        catch { return "某修士"; }
    }

    internal static string[] SplitTags(string value) => NormalizeTags((value ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

    internal static string[] NormalizeTags(IEnumerable<string> tags)
    {
        string[] result = (tags ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.Ordinal).Take(3).ToArray();
        return result.Length == 0 ? new[] { "灵" } : result;
    }

    private static string Pick(string[] values, int hash) => values[(hash & int.MaxValue) % values.Length];
    private static string Collapse(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "无名玄物";
        return value.Replace("玄玄", "玄").Replace("灵灵", "灵").Replace("天天", "天").Replace("空空", "空").Replace("火火", "火").Replace("水水", "水");
    }

    private static string StemBranch(int value)
    {
        string[] stems = { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };
        string[] branches = { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };
        int n = value & int.MaxValue;
        return stems[n % stems.Length] + branches[n % branches.Length];
    }


    private static string ChineseNumber(int value)
    {
        string[] digits = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        int n = Math.Max(0, value);
        if (n < 10) return digits[n];
        if (n < 20) return "十" + (n % 10 == 0 ? string.Empty : digits[n % 10]);
        if (n < 100) return digits[n / 10] + "十" + (n % 10 == 0 ? string.Empty : digits[n % 10]);
        string text = string.Empty;
        foreach (char c in n.ToString()) text += digits[c - '0'];
        return text;
    }

    private static int PositiveHash(string value) => StableHash(value) & int.MaxValue;
    private static int StableHash(string value)
    {
        unchecked { int hash = 29; foreach (char c in value ?? string.Empty) hash = hash * 41 + c; return hash; }
    }
}
