using System;
using System.Text;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslActorInfoFormatter
{
    internal static string Format(Actor actor)
    {
        MclslActorCultivationView cultivation = MclslActorCultivationQuery.Build(actor);
        if (!cultivation.Alive) return "<color=#A6D8D1>─ 角色概览 ─────────</color>\n  暂无角色信息";
        if (!cultivation.HasCultivationData) return string.Empty;

        string realm = cultivation.RealmId;
        string displayRealm = DisplayRealmName(cultivation);
        StringBuilder b = new(720);
        b.Append(Title(string.IsNullOrWhiteSpace(cultivation.CultivationSystemName) || cultivation.CultivationSystemName == "未入道" ? "修行档案" : cultivation.CultivationSystemName + "档案"));
        b.Append(Section("基础"));
        b.Append(Line("境界", Highlight(displayRealm, string.IsNullOrWhiteSpace(realm) ? "#9CD7FF" : RealmColor(realm))));
        if (!string.IsNullOrWhiteSpace(realm))
            b.Append(Line("体系", Highlight(cultivation.CultivationSystemName, cultivation.IsSpiritualRootPath ? "#A6D8D1" : "#FFD37A")));
        if (MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear()))
            b.Append(Line("所属组织", Highlight(FactionDisplay(cultivation.FactionAffiliation), "#B7A7FF")));
        AppendFormerLife(b, actor);
        if (cultivation.IsSpiritualRootPath)
        {
            b.Append(Line("状态", string.IsNullOrWhiteSpace(cultivation.AncientLawStatus) ? DefaultSpiritualStatus(cultivation.CultivationSystemName) : cultivation.AncientLawStatus));
            b.Append(CompactLine("法脉", PercentColor(cultivation.AncientLineageStrength), "传承", PercentColor(cultivation.AncientLegacyPotential)));
            string mentorship = MclslAncientMentorshipSystem.BuildSummary(actor);
            if (!string.IsNullOrWhiteSpace(mentorship))
                b.Append(Line("师徒", mentorship));
        }

        if (cultivation.HasSpiritualRoot)
        {
            b.Append(Section("灵根"));
            b.Append(Line("品阶", Highlight(cultivation.SpiritualRootGrade, "#D8C778")));
            b.Append(Line("属性", ReplaceTags(cultivation.SpiritualRootAttributes)));
            b.Append(Line("数量", RootCountText(cultivation.SpiritualRootCountName)));
            b.Append(Line("纯度", PercentColor(cultivation.SpiritualRootPurity)));
        }

        b.Append(Section("修行"));
        if (string.IsNullOrWhiteSpace(realm))
        {
            b.Append(Line("炼心", cultivation.HeartMethodText));
            b.Append(Line("炼心进度", PercentColor(cultivation.HeartTemperingProgress)));
            if (cultivation.HasCultivationData)
                b.Append(Line("真元", SensingQiEssenceText(cultivation.TrueEssence)));
            if (HasHuanzhen(actor)) AppendHuanzhen(b, actor);
            return b.ToString().TrimEnd();
        }

        string techniqueMaxRealm = MclslTechniqueRealmLimit.DisplayMaxRealm(actor);
        string techniqueText = Highlight(cultivation.TechniqueName, "#F1D17A");
        if (!string.IsNullOrWhiteSpace(techniqueMaxRealm))
            techniqueText += " <color=#6F7B86>最高可至" + techniqueMaxRealm + "</color>";
        b.Append(Line("功法", techniqueText));
        AppendDaoStruggleLines(b, actor, cultivation);
        b.Append(CompactLine("境界进度", ProgressText(cultivation), "真元", EssenceText(cultivation)));
        b.Append(Line("心境", PercentColor(cultivation.MindState)));
        b.Append(Line("炼心", cultivation.HeartMethodText + " " + PercentColor(cultivation.HeartTemperingProgress)));
        if (ShouldShowMiasma(cultivation) && cultivation.MortalMiasmaLimit > 0)
            b.Append(Line("仙凡瘴", CountColor(cultivation.MortalMiasma, cultivation.MortalMiasmaLimit) + " <color=#6F7B86>/ " + cultivation.MortalMiasmaLimit + "</color>"));

        if (!cultivation.IsSpiritualRootPath)
        {
            b.Append(Section("新法根基"));
            b.Append(Line("根基完整", PercentColor(cultivation.LineageIntegrity)));
            b.Append(Line("根基", CompactLineage(cultivation)));
        }
        AppendCurrentStage(b, cultivation);

        if (HasHuanzhen(actor)) AppendHuanzhen(b, actor);
        return b.ToString().TrimEnd();
    }

    private static bool ShouldShowMiasma(MclslActorCultivationView cultivation)
    {
        if (cultivation == null || cultivation.IsSpiritualRootPath) return false;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return run != null && !string.Equals(run.CultivationEpoch, MclslWorldEpochSystem.AncientLawEpoch, StringComparison.Ordinal);
    }

    private static void AppendDaoStruggleLines(StringBuilder b, Actor actor, MclslActorCultivationView cultivation)
    {
        if (actor?.data == null || cultivation == null) return;
        if (!MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear())) return;

        string text = MclslTechniqueOccupationSystem.ConflictDisplayText(actor);
        if (!string.IsNullOrWhiteSpace(text))
            b.Append(Line("同法", Highlight(text, text.Contains("不可同修", StringComparison.Ordinal) ? "#FFD37A" : "#A7E08A")));
    }

    private static string MclslTechniqueLineageName(string lineageId)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.TechniqueLineages == null || string.IsNullOrWhiteSpace(lineageId)) return "未定";
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord lineage = run.TechniqueLineages[i];
            if (lineage != null && string.Equals(lineage.Id, lineageId, StringComparison.Ordinal))
                return string.IsNullOrWhiteSpace(lineage.Name) ? lineageId : lineage.Name;
        }
        return lineageId;
    }

    private static void AppendCurrentStage(StringBuilder b, MclslActorCultivationView cultivation)
    {
        if (cultivation.IsSpiritualRootPath)
        {
            AppendSpiritualPathStage(b, cultivation);
            return;
        }
        string realm = cultivation.RealmId;
        if (realm == MclslRealmIds.LianQi)
        {
            return;
        }
        if (realm == MclslRealmIds.ZhuJi)
        {
            AppendStageLine(b, "筑基", cultivation.FoundationWonderName, FoundationWonderRank(cultivation), cultivation.FoundationWonderTags);
            return;
        }
        if (realm == MclslRealmIds.JinDan)
        {
            AppendStageLine(b, "金丹", cultivation.GoldenCoreLaws, cultivation.GoldenCorePurity + "%纯", cultivation.GoldenCoreStability + "%协");
            return;
        }
        if (realm == MclslRealmIds.YuanYing)
        {
            AppendStageLine(b, "元婴", cultivation.NascentCaveName, cultivation.NascentCaveCompatibility + "%适配", "洞天" + cultivation.NascentCaveIntegrity + "%");
            if (!string.IsNullOrWhiteSpace(cultivation.NascentEssenceName)) b.Append(Line("天地之精", cultivation.NascentEssenceName));
            return;
        }
        if (realm == MclslRealmIds.HuaShen)
        {
            AppendStageLine(b, "化神", cultivation.DivineChangeName, cultivation.DivineChangeCompatibility + "%适配", cultivation.DivineMarrowName);
            return;
        }
        if (realm == MclslRealmIds.HeDao)
        {
            string dutyState = string.IsNullOrWhiteSpace(cultivation.HeavenlyDuty)
                ? string.Empty
                : cultivation.HeavenlyDuty;
            AppendStageLine(b, "合道", cultivation.WorldSoulName, dutyState, cultivation.HarmonyStability + "%稳");
            if (!string.IsNullOrWhiteSpace(cultivation.HeavenlyDuty))
            {
                b.Append(Line("天职进度", cultivation.HeavenlyDutyProgress + "%"));
                b.Append(Line("天职反噬", cultivation.HeavenlyDutyBacklash + "%"));
            }
            if (!string.IsNullOrWhiteSpace(cultivation.InverseTruthName))
                b.Append(Line("天地之理", cultivation.InverseTruthName + " " + MclslInverseTruthStageCatalog.StageName(cultivation.InverseTruthProgress) + " " + cultivation.InverseTruthProgress + "%"));
            return;
        }
        if (realm == MclslRealmIds.ChangSheng)
        {
            string stage = cultivation.TaishangProgress >= 100 ? "太上" : "长生";
            AppendStageLine(b, stage, cultivation.InverseTruthName, MclslInverseTruthStageCatalog.StageName(cultivation.InverseTruthProgress), "太上进度 " + PercentColor(cultivation.TaishangProgress));
        }
    }

    private static void AppendSpiritualPathStage(StringBuilder b, MclslActorCultivationView cultivation)
    {
        b.Append(Section("仙道根基"));
        if (cultivation.TechniqueComprehensionProgress > 0)
            AppendTechniqueComprehensionLines(b, cultivation.TechniqueComprehensionProgress);

        AppendAncientSummaryBlock(b, "筑基", cultivation.AncientFoundationName,
            ("品质", AncientQuality(cultivation.AncientFoundationQuality)),
            ("稳定", cultivation.AncientFoundationStability <= 0 ? string.Empty : PercentColor(cultivation.AncientFoundationStability)));
        AppendAncientSummaryBlock(b, "金丹", cultivation.AncientCoreName,
            ("本命", cultivation.AncientDaoIntent),
            ("纯度", cultivation.AncientCorePurity <= 0 ? string.Empty : PercentColor(cultivation.AncientCorePurity)));
        AppendAncientSummaryBlock(b, "元婴", cultivation.AncientNascentName,
            ("神魂", cultivation.AncientSoulStrength <= 0 ? string.Empty : PercentColor(cultivation.AncientSoulStrength)),
            ("肉身", cultivation.AncientBodyFit <= 0 ? string.Empty : PercentColor(cultivation.AncientBodyFit)));
        AppendAncientSummaryBlock(b, "化神", cultivation.AncientDivineIntent,
            ("神魂化度", cultivation.AncientSoulFusion <= 0 ? string.Empty : PercentColor(cultivation.AncientSoulFusion)),
            ("大道契合", cultivation.AncientDaoCompatibility <= 0 ? string.Empty : PercentColor(cultivation.AncientDaoCompatibility)));
        AppendAncientSummaryBlock(b, "合道", cultivation.AncientDaoName,
            ("大道完整", cultivation.AncientHarmonyIntegrity <= 0 ? string.Empty : PercentColor(cultivation.AncientHarmonyIntegrity)),
            ("天地契合", cultivation.AncientHeavenCompatibility <= 0 ? string.Empty : PercentColor(cultivation.AncientHeavenCompatibility)));
    }

    private static void AppendTechniqueComprehensionLines(StringBuilder b, int progress)
    {
        int value = Math.Clamp(progress, 0, 100);
        b.Append(Line("功法参悟", MclslTechniqueStageSystem.StageName(value) + PercentColor(value)));
        b.Append(Line("功法效率", PercentColor(MclslTechniqueStageSystem.EfficiencyPercent(value), 140)));
    }

    private static string DefaultSpiritualStatus(string display) => display switch
    {
        "仙道" => "仙道正修",
        "旧法" => "旧法遗修",
        _ => "古法遗修"
    };

    private static string FactionDisplay(string faction) => faction switch
    {
        "wanxian" => "万仙盟",
        "five_elders" => "五老会",
        "万仙盟" => "万仙盟",
        "五老会" => "五老会",
        _ => "未入盟"
    };

    private static string AncientQuality(int quality) => quality switch { >= 4 => "上乘", 3 => "精纯", 2 => "稳固", 1 => "初成", _ => string.Empty };
    private static string PercentText(string label, int value) => value <= 0 ? string.Empty : label + value + "%";

    private static void AppendAncientBlock(StringBuilder b, string label, string name, params (string Label, string Value)[] fields)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        b.Append(Line(label, Highlight(ReplaceTags(name), "#F1D17A")));
        if (fields == null) return;
        for (int i = 0; i < fields.Length; i++)
        {
            string fieldLabel = fields[i].Label;
            string fieldValue = fields[i].Value;
            if (string.IsNullOrWhiteSpace(fieldLabel) || string.IsNullOrWhiteSpace(fieldValue)) continue;
            b.Append(Line("  " + fieldLabel, ReplaceTags(fieldValue)));
        }
    }

    private static void AppendAncientSummaryBlock(StringBuilder b, string label, string name, params (string Label, string Value)[] fields)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        b.Append("\n  <color=#9CD7FF><b>" + label + "</b></color>\n");
        b.Append("    <color=#D8CDAA>根基</color> " + Highlight(ReplaceTags(name), "#F1D17A") + "\n");
        if (fields == null) return;
        for (int i = 0; i < fields.Length; i++)
        {
            string fieldLabel = fields[i].Label;
            string fieldValue = fields[i].Value;
            if (string.IsNullOrWhiteSpace(fieldLabel) || string.IsNullOrWhiteSpace(fieldValue)) continue;
            b.Append("    <color=#8FA2A8>" + fieldLabel + "</color> " + ReplaceTags(fieldValue) + "\n");
        }
    }

    private static string CompactLineage(MclslActorCultivationView cultivation)
    {
        if (!string.IsNullOrWhiteSpace(cultivation.WorldSoulName)) return "魄:" + cultivation.WorldSoulName;
        if (!string.IsNullOrWhiteSpace(cultivation.DivineMarrowName)) return "髓:" + cultivation.DivineMarrowName;
        if (!string.IsNullOrWhiteSpace(cultivation.NascentEssenceName)) return "精:" + cultivation.NascentEssenceName;
        if (!string.IsNullOrWhiteSpace(cultivation.GoldenCoreLaws)) return "法:" + ReplaceTags(cultivation.GoldenCoreLaws);
        if (!string.IsNullOrWhiteSpace(cultivation.FoundationWonderName)) return "基:" + cultivation.FoundationWonderName;
        return "未成根基";
    }

    private static void AppendStageLine(StringBuilder b, string label, string primary, string secondary, string tertiary)
    {
        if (string.IsNullOrWhiteSpace(primary)) return;
        string text = ReplaceTags(primary);
        if (!string.IsNullOrWhiteSpace(secondary) && secondary != "0%" && secondary != "%") text += "\n    " + ReplaceTags(secondary);
        if (!string.IsNullOrWhiteSpace(tertiary)) text += "\n    " + ReplaceTags(tertiary);
        b.Append(Line(label, text));
    }

    private static void AppendHuanzhen(StringBuilder b, Actor actor)
    {
        b.Append(Section("还真"));
        b.Append(Line("状态", MclslRuntimeSettings.HuanzhenEnabled ? Highlight("已启用", "#A7E08A") : "设置中关闭"));
        int anchor = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HuanzhenAnchorYear, -1);
        b.Append(Line("锚点", anchor < 0 ? "尚未建立" : anchor + "年"));
        b.Append(Line("回溯", MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HuanzhenRestoreCount, 0).ToString()));
    }

    private static void AppendFormerLife(StringBuilder b, Actor actor)
    {
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ReincarnationApplied, 0) != 1) return;
        string sourceName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.ReincarnationSourceName, string.Empty);
        string sourceRealm = MclslRealmIds.Display(MclslActorAccessor.GetString(actor, MclslActorDataKeys.ReincarnationSourceRealm, string.Empty));
        int deathYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ReincarnationDeathYear, 0);
        string technique = MclslActorAccessor.GetString(actor, MclslActorDataKeys.ReincarnationTechniqueName, string.Empty);
        string text = string.IsNullOrWhiteSpace(sourceName) ? "前尘未明" : sourceName + "（" + sourceRealm + (deathYear > 0 ? "，" + deathYear + "年身陨" : string.Empty) + "）";
        b.Append(Line("前世", Highlight(text, "#C9B8FF")));
        if (!string.IsNullOrWhiteSpace(technique))
            b.Append(Line("前世功法", Highlight("《" + technique + "》", "#D8C778")));
    }

    private static bool HasHuanzhen(Actor actor)
    {
        try { return actor != null && actor.hasTrait(MySimulatedLongevityRoad.Traits.MclslTraitRegistration.HuanzhenTraitId); }
        catch { return false; }
    }

    private static string DisplayRealmName(MclslActorCultivationView cultivation)
    {
        if (cultivation == null) return "凡俗";
        if (string.IsNullOrWhiteSpace(cultivation.RealmId) && TrySensingQiStage(cultivation.TrueEssence, out string stage))
            return "感气：" + stage;
        return cultivation.RealmName;
    }

    private static string SensingQiEssenceText(int trueEssence)
    {
        int requirement = SensingQiRequirement();
        return "<color=#9CD7FF>" + Math.Max(0, trueEssence) + "</color><color=#6F7B86>/" + requirement + "</color>";
    }

    private static bool TrySensingQiStage(int trueEssence, out string stage)
    {
        stage = string.Empty;
        int requirement = SensingQiRequirement();
        if (requirement <= 0 || trueEssence <= 0) return false;

        int tenths = (int)Math.Floor(trueEssence * 10f / requirement);
        tenths = Math.Clamp(tenths <= 0 ? 1 : tenths, 1, 10);
        stage = ChengText(tenths);
        return true;
    }

    private static int SensingQiRequirement()
    {
        return MclslRealmProgress.LianQiEntryMinimum;
    }

    private static string ChengText(int tenths) => tenths switch
    {
        1 => "一成",
        2 => "二成",
        3 => "三成",
        4 => "四成",
        5 => "五成",
        6 => "六成",
        7 => "七成",
        8 => "八成",
        9 => "九成",
        _ => "十成"
    };

    private static string ReplaceTags(string value) => string.IsNullOrWhiteSpace(value) ? "无" : value.Replace(",", "、");
    private static string RootCountText(string value) => string.IsNullOrWhiteSpace(value) ? "无" : value == "单灵根" ? "单灵根" : value + "（多灵根）";
    private static string ShortText(string value, int max)
    {
        string text = ReplaceTags(value).Replace("\n", " ").Trim();
        if (text.Length <= max) return text;
        return text.Substring(0, Math.Max(1, max - 1)) + "…";
    }
    private static string FoundationWonderRank(MclslActorCultivationView cultivation) =>
        MclslGeneratedObjectFactory.FoundationRankText(cultivation.FoundationWonderCategory, cultivation.FoundationWonderGrade, cultivation.FoundationWonderCompleteness, cultivation.FoundationWonderRuleStrength, cultivation.FoundationWonderQuality);
    private static string Title(string title) => "<color=#A6D8D1><b>◇ " + title + "</b></color>\n";
    private static string Section(string title) => "\n<color=#B9EEE4><b>┄ " + title + " ┄</b></color>\n";
    private static string Line(string label, string value) => "  <color=#D8CDAA>" + label + "</color> <color=#596A70>·</color> " + (string.IsNullOrWhiteSpace(value) ? "无" : value) + "\n";
    private static string CompactLine(string leftLabel, string leftValue, string rightLabel, string rightValue) =>
        Line(leftLabel, leftValue) + Line(rightLabel, rightValue);
    private static string Highlight(string value, string color) => string.IsNullOrWhiteSpace(value) ? "无" : "<color=" + color + ">" + value + "</color>";
    private static string PercentColor(int value, int max = 100)
    {
        int clamped = Math.Clamp(value, 0, Math.Max(1, max));
        string color = max > 100 && clamped >= max * 8 / 10 ? "#FF8877" : clamped >= max * 7 / 10 ? "#A7E08A" : clamped >= max * 4 / 10 ? "#FFD37A" : "#9CD7FF";
        return "<color=" + color + ">" + clamped + "%</color>";
    }
    private static string CountColor(int value, int max)
    {
        int clamped = Math.Clamp(value, 0, Math.Max(1, max));
        string color = clamped >= max * 8 / 10 ? "#FF8877" : clamped >= max * 5 / 10 ? "#FFD37A" : "#9CD7FF";
        return "<color=" + color + ">" + clamped + "</color>";
    }
    private static string ProgressText(MclslActorCultivationView cultivation) => cultivation.RealmId == MclslRealmIds.ChangSheng ? PercentColor(cultivation.TaishangProgress) : cultivation.NextRealmMinimum <= 0 ? "-" : PercentColor((int)Math.Round(cultivation.CultivationProgress));
    private static string EssenceText(MclslActorCultivationView cultivation) => cultivation.NextRealmMinimum <= 0
        ? "<color=#9CD7FF>" + cultivation.TrueEssence + "</color>"
        : "<color=#9CD7FF>" + cultivation.TrueEssence + "</color><color=#6F7B86>（下境最低" + cultivation.NextRealmMinimum + "）</color>";
    private static string RealmColor(string realm) => realm switch
    {
        MclslRealmIds.LianQi => "#9CD7FF",
        MclslRealmIds.ZhuJi => "#A7E08A",
        MclslRealmIds.JinDan => "#FFD37A",
        MclslRealmIds.YuanYing => "#D8C778",
        MclslRealmIds.HuaShen => "#FF9B6A",
        MclslRealmIds.HeDao => "#B7A7FF",
        MclslRealmIds.ChangSheng => "#F6F0A8",
        _ => "#CFC7B2"
    };
}
