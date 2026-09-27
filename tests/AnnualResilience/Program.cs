using System.Text.Json;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Patches;
using MySimulatedLongevityRoad.Systems;

static void Check(bool condition, string caseName)
{
    if (!condition) throw new Exception(caseName);
    Console.WriteLine("PASS " + caseName);
}

for (int grade = 0; grade <= 4; grade++)
{
    int minimum = Math.Max(0, grade - 1);
    Check(MclslProfessionCraftingPolicy.MinimumRecipeGrade(grade) == minimum,
        $"职业品阶 {grade} 最多降一阶");
    for (int recipeGrade = 0; recipeGrade <= 4; recipeGrade++)
    {
        bool expectedCandidate = recipeGrade >= minimum && recipeGrade <= grade;
        Check(MclslProfessionCraftingPolicy.CanAttemptRecipe(grade, recipeGrade) == expectedCandidate,
            $"职业品阶 {grade} 的配方候选边界 {recipeGrade}");
        int expectedExperience = recipeGrade == grade ? 1 : 0;
        Check(MclslProfessionCraftingPolicy.ExperienceGain(grade, recipeGrade) == expectedExperience,
            $"职业品阶 {grade} 制作 {recipeGrade} 阶熟练度");
    }
}

foreach (bool historyEnabled in new[] { false, true })
foreach (bool lowGradeEnabled in new[] { false, true })
{
    for (int grade = 0; grade <= 4; grade++)
    {
        bool expected = historyEnabled && (grade >= 3 || lowGradeEnabled);
        foreach (string category in new[] { "Pill", "Talisman", "Artifact" })
            Check(MclslItemAcquisitionHistoryPolicy.ShouldRecordFinishedProduct(
                    category, grade, historyEnabled, lowGradeEnabled) == expected,
                $"成品历史开关矩阵 {category}/{grade}/{historyEnabled}/{lowGradeEnabled}");
    }
    for (int tier = 1; tier <= 4; tier++)
    {
        bool expected = historyEnabled && (tier >= 3 || lowGradeEnabled);
        Check(MclslItemAcquisitionHistoryPolicy.ShouldRecordMaterial(
                tier, historyEnabled, lowGradeEnabled) == expected,
            $"材料历史开关矩阵 {tier}/{historyEnabled}/{lowGradeEnabled}");
    }
}
Check(!MclslItemAcquisitionHistoryPolicy.ShouldRecordFinishedProduct("SpellScroll", 4, true, true),
    "法术卷轴不属于职业成品历史");
Check(!MclslItemAcquisitionHistoryPolicy.ShouldRecordFinishedProduct("Material", 4, true, true),
    "材料不走成品历史入口");

Check(MclslAnnualResiliencePolicy.ShouldRetry(1, false), "第一次安全失败可重试");
Check(MclslAnnualResiliencePolicy.ShouldRetry(2, false), "第二次安全失败可重试");
Check(!MclslAnnualResiliencePolicy.ShouldRetry(3, false), "第三次安全失败隔离");
Check(!MclslAnnualResiliencePolicy.ShouldRetry(1, true), "副作用开始后禁止整步重放");
Check(MclslAnnualResiliencePolicy.RetryAtFrame(100) == 130, "重试间隔三十帧");
Check(MclslAnnualResiliencePolicy.RetryAtFrame(int.MaxValue - 5) == int.MaxValue, "帧号边界");

static string? ParseJson(string json)
{
    try
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.ValueKind == JsonValueKind.Object ? json : null;
    }
    catch (JsonException) { return null; }
}

Check(MclslPrimaryBackupRecovery.Select("{\"item\":1}", "{\"item\":2}", ParseJson,
    out string? primary) == MclslRecoverySource.Primary && primary == "{\"item\":1}",
    "有效主数据优先");
Check(MclslPrimaryBackupRecovery.Select("{broken", "{\"item\":2}", ParseJson,
    out string? restored) == MclslRecoverySource.Backup && restored == "{\"item\":2}",
    "损坏主数据使用有效备份");
Check(MclslPrimaryBackupRecovery.Select("", "{\"item\":2}", ParseJson,
    out string? missing) == MclslRecoverySource.Backup && missing != null,
    "主数据缺失时恢复备份");
Check(MclslPrimaryBackupRecovery.Select("{broken", "{also-broken", ParseJson,
    out string? locked) == MclslRecoverySource.Locked && locked == null,
    "双份损坏锁定写入");

List<string> pending = MclslAnnualTargetCursor.Create(new[] { "z", "b", "a", "b" });
Check(pending.SequenceEqual(new[] { "a", "b", "z" }), "目标 ID 排序并去重");
MclslAnnualTargetCursor.Complete(pending, "a");
string saved = JsonSerializer.Serialize(pending);
pending = JsonSerializer.Deserialize<List<string>>(saved)!;
Check(pending.SequenceEqual(new[] { "b", "z" }), "结算中途存读档保留未完成目标");
MclslAnnualTargetCursor.Complete(pending, "b");
Check(pending.SequenceEqual(new[] { "z" }), "单目标失败后隔离并继续其余目标");
bool refusedWrongCursor = false;
try { MclslAnnualTargetCursor.Complete(pending, "other"); }
catch (InvalidOperationException) { refusedWrongCursor = true; }
Check(refusedWrongCursor && pending.SequenceEqual(new[] { "z" }), "游标不匹配时不误删目标");

MclslWorldArchiveBundle oldArchive = JsonSerializer.Deserialize<MclslWorldArchiveBundle>(
    "{\"Version\":20,\"CurrentRun\":{\"AnnualBatch\":{\"ActiveYear\":42,\"WorldStage\":5}}}")!;
Check(oldArchive.CurrentRun.AnnualBatch.ActiveYear == 42
    && oldArchive.CurrentRun.AnnualBatch.WorldStage == 5
    && oldArchive.CurrentRun.AnnualBatch.FailureRecords.Count == 0
    && oldArchive.CurrentRun.AnnualBatch.TechniqueLineagePendingIds.Count == 0
    && !oldArchive.CurrentRun.AnnualBatch.TechniqueLineagePendingInitialized
    && oldArchive.CurrentRun.AnnualBatch.SectLifecycleCursor == 0,
    "旧档缺少新增游标和失败字段时采用安全默认值");

Check(!MclslHarmonyPatchGuard.TryPatch(new HarmonyLib.Harmony(), "annual_core_test", null)
    && MclslHarmonyPatchGuard.FailedRequiredCount == 1
    && MclslHarmonyPatchGuard.FailedRequiredSummary.Contains("annual_core_test")
    && MclslDiagnostics.Errors.Any(x => x.Contains("annual_core_test")),
    "关键补丁目标缺失时记录名称并警告");
