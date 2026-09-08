using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslHuanzhenSystem
{
    private const int MaxAnchors = 3;
    private static MclslHuanzhenExternalState _state = new();
    private static bool _loaded;
    private static bool _applyingRestore;
    private static bool _rollbackQueued;
    private static int _rollbackDelayFrames;
    private static bool _rollbackLoadInProgress;
    private static bool _anyWorldLoadInProgress;
    private static string StatePath => Path.Combine(Application.persistentDataPath, "MySimulatedLongevityRoad", "HuanzhenState.json");

    internal static bool IsApplyingRestore => _applyingRestore;
    internal static bool IsRollbackLoadInProgress => _rollbackLoadInProgress;
    internal static MclslHuanzhenExternalState Current { get { EnsureLoaded(); return _state; } }

    internal static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            if (File.Exists(StatePath)) _state = JsonConvert.DeserializeObject<MclslHuanzhenExternalState>(File.ReadAllText(StatePath)) ?? new();
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路][还真] 外部状态读取失败: " + ex.Message); }
        Normalize();
    }

    internal static void Tick()
    {
        EnsureLoaded();
        if (!_rollbackQueued || _rollbackLoadInProgress) return;
        if (_rollbackDelayFrames-- > 0) return;
        _rollbackQueued = false;
        MclslHuanzhenPendingRestore pending = _state.PendingRestore;
        if (pending == null || !pending.Active || string.IsNullOrWhiteSpace(pending.RelativeSavePath)) return;
        try
        {
            _rollbackLoadInProgress = true;
            MclslRuntime.ClearWorldState();
            if (World.world?.save_manager == null)
                throw new InvalidOperationException("SaveManager实例尚未就绪");
            World.world.save_manager.loadWorld(pending.RelativeSavePath, false);
        }
        catch (Exception ex)
        {
            _rollbackLoadInProgress = false;
            pending.Active = false;
            AddHistory(pending.DeathYear, pending.AnchorYear, pending.HostName, pending.Cultivation?.RealmId, pending.LoopDepth, "读档失败：" + ex.Message);
            Flush();
            Debug.LogError("[模拟长生路][还真] 回载锚点失败: " + ex);
        }
    }

    internal static void TickAnnual(int year)
    {
        EnsureLoaded();
        if (!MclslRuntimeSettings.HuanzhenEnabled || _rollbackLoadInProgress || _state.PendingRestore?.Active == true) return;
        Actor host = FindLivingHost();
        if (host == null) return;
        BindHost(host, false);
        if (!IsSafeToAnchor(host, year)) return;
        TryCreateAnchor(host, year);
    }

    internal static void OnTraitGranted(Actor actor)
    {
        if (_applyingRestore || actor?.data == null) return;
        EnsureLoaded();
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (units != null)
        {
            int checkedHosts = 0;
            for (int i = 0; i < units.Count && checkedHosts < 32; i++)
            {
                Actor other = units[i];
                if (other == null || other == actor || other.data == null) continue;
                if (!HasHuanzhenTrait(other)) continue;
                checkedHosts++;
                try { if (other.hasTrait(MclslTraitRegistration.HuanzhenTraitId)) other.removeTrait(MclslTraitRegistration.HuanzhenTraitId); }
                catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #1: " + mclslEmptyCatchEx.Message); }
            }
        }
        BindHost(actor, true);
        string status = MclslRuntimeSettings.HuanzhenEnabled ? "还真已启用，将在安全年份建立滚动锚点。" : "还真特质已绑定，但设置中的“启用还真”当前关闭。";
        MclslAnnouncementSystem.Enqueue(MclslActorAccessor.DisplayName(actor) + "成为唯一还真持有者。" + status, "#7CCFD0", 9f, 1);
    }

    internal static MclslHuanzhenDeathSnapshot CaptureDeath(Actor actor)
    {
        if (!MclslRuntimeSettings.HuanzhenEnabled || actor?.data == null) return MclslHuanzhenDeathSnapshot.Empty;
        bool has;
        try { has = actor.hasTrait(MclslTraitRegistration.HuanzhenTraitId); } catch { has = false; }
        if (!has) return MclslHuanzhenDeathSnapshot.Empty;
        string identity = EnsureIdentity(actor);
        return new MclslHuanzhenDeathSnapshot(true, identity, MclslActorAccessor.Id(actor), SafeName(actor), MclslRuntime.CurrentYear(), CaptureCultivation(actor));
    }

    internal static void CommitDeath(Actor actor, MclslHuanzhenDeathSnapshot snapshot)
    {
        if (!snapshot.Found || actor?.data == null) return;
        bool dead;
        try { dead = !actor.isAlive(); } catch { dead = false; }
        if (!dead) return;
        EnsureLoaded();
        List<MclslHuanzhenAnchorRecord> anchors = (_state.Anchors ?? new List<MclslHuanzhenAnchorRecord>())
            .Where(x => x != null && x.HostIdentity == snapshot.Identity && x.Year <= snapshot.DeathYear && !string.IsNullOrWhiteSpace(x.RelativeSavePath))
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Sequence).ToList();
        if (anchors.Count == 0)
        {
            AddHistory(snapshot.DeathYear, -1, snapshot.ActorName, snapshot.Cultivation.RealmId, 0, "无可用锚点，还真未发动");
            Flush();
            MclslAnnouncementSystem.Enqueue(snapshot.ActorName + "身负还真却未留下可用锚点，此世仍告终结。", "#8FA6AE", 9f, 1);
            return;
        }

        int safetyGap = MclslRuntimeSettings.HuanzhenSafetyGapYears;
        int loopWindow = Math.Max(safetyGap, MclslRuntimeSettings.HuanzhenAnchorIntervalYears);
        bool rapidRepeat = _state.LastRestoreYear >= 0 && snapshot.DeathYear - _state.LastRestoreYear < loopWindow;
        _state.ConsecutiveLoopDeaths = rapidRepeat ? Math.Min(MaxAnchors - 1, _state.ConsecutiveLoopDeaths + 1) : 0;

        List<MclslHuanzhenAnchorRecord> safe = anchors
            .Where(x => snapshot.DeathYear - x.Year >= safetyGap)
            .Where(x => !rapidRepeat || x.Year < _state.LastRestoreYear)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Sequence)
            .ToList();
        if (safe.Count == 0)
        {
            int nearestYear = anchors.Count == 0 ? -1 : anchors.Max(x => x.Year);
            AddHistory(snapshot.DeathYear, nearestYear, snapshot.ActorName, snapshot.Cultivation.RealmId, _state.ConsecutiveLoopDeaths, rapidRepeat ? "连续死亡且无更老安全锚点，拒绝回滚" : "全部锚点过近，拒绝回滚");
            Flush();
            string reason = rapidRepeat ? "连续死亡后已无更老的安全锚点" : "死亡距现有锚点均过近";
            MclslAnnouncementSystem.Enqueue(snapshot.ActorName + reason + "，为避免还真死循环，本次还真不发动。", "#C08763", 10f, 1);
            return;
        }
        MclslHuanzhenAnchorRecord selected = safe[0];
        int index = _state.ConsecutiveLoopDeaths;
        _state.PendingRestore = new MclslHuanzhenPendingRestore
        {
            Active = true,
            RelativeSavePath = selected.RelativeSavePath,
            OriginalSavePath = _state.OriginalSavePath,
            WorldRunId = _state.WorldRunId,
            HostIdentity = snapshot.Identity,
            HostName = snapshot.ActorName,
            AnchorYear = selected.Year,
            DeathYear = snapshot.DeathYear,
            LoopDepth = index,
            Cultivation = snapshot.Cultivation
        };
        _state.Anchors = anchors.Where(x => x.Year <= selected.Year).OrderByDescending(x => x.Year).Take(MaxAnchors).ToList();
        Flush();
        _rollbackQueued = true;
        _rollbackDelayFrames = 2;
        MclslAnnouncementSystem.Enqueue(snapshot.ActorName + "身死，还真发动：将由" + snapshot.DeathYear + "年回溯至" + selected.Year + "年。", "#7CCFD0", 10f, 1);
    }

    internal static void PrepareForNewWorld()
    {
        EnsureLoaded();
        // SaveManager.loadWorld 的内部流程可能经过地图初始化入口；读档期间绝不能把已有锚点当作新世界数据删除。
        if (_anyWorldLoadInProgress || _state.PendingRestore?.Active == true) return;
        DeleteAnchorFolders(_state.Anchors);
        _state = new MclslHuanzhenExternalState();
        ClearRuntime();
        Flush();
    }

    internal static void PrepareForAnyWorldLoad(string path)
    {
        EnsureLoaded();
        _anyWorldLoadInProgress = true;
        if (_state.PendingRestore?.Active == true && string.Equals(_state.PendingRestore.RelativeSavePath, path ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            _rollbackLoadInProgress = true;
    }

    internal static void OnWorldLoaded()
    {
        EnsureLoaded();
        _anyWorldLoadInProgress = false;
        MclslHuanzhenPendingRestore pending = _state.PendingRestore;
        if (pending == null || !pending.Active)
        {
            _rollbackLoadInProgress = false;
            return;
        }

        string loadedPath = SaveManager.currentSavePath ?? string.Empty;
        if (!string.Equals(loadedPath, pending.RelativeSavePath, StringComparison.OrdinalIgnoreCase))
        {
            // 上次死亡后若游戏在真正回载前退出，PendingRestore 会保留；下一次进入任意世界时重新排队加载锚点，不能直接把死前修为注入错误存档。
            if (SaveManager.doesSaveExist(pending.RelativeSavePath))
            {
                _rollbackLoadInProgress = false;
                _rollbackQueued = true;
                _rollbackDelayFrames = 2;
                return;
            }
            pending.Active = false;
            _rollbackLoadInProgress = false;
            AddHistory(pending.DeathYear, pending.AnchorYear, pending.HostName, pending.Cultivation?.RealmId, pending.LoopDepth, "锚点存档不存在，取消还真");
            Flush();
            return;
        }

        Actor host = FindByIdentity(pending.HostIdentity);
        if (host == null)
        {
            pending.Active = false;
            _rollbackLoadInProgress = false;
            AddHistory(pending.DeathYear, pending.AnchorYear, pending.HostName, pending.Cultivation?.RealmId, pending.LoopDepth, "锚点中未找到还真持有者");
            Flush();
            return;
        }
        try
        {
            _applyingRestore = true;
            if (!host.hasTrait(MclslTraitRegistration.HuanzhenTraitId))
            {
                ActorTrait huanzhen = AssetManager.traits.get(MclslTraitRegistration.HuanzhenTraitId);
                if (huanzhen != null) host.addTrait(huanzhen, true);
            }
            MclslCultivationSystem.RestoreFromHuanzhen(host, pending.Cultivation, pending.AnchorYear, pending.DeathYear);
            MclslWorldSoulSystem.ReconcileRestoredHolder(host, pending.Cultivation, MclslRuntime.CurrentYear());
            int count = MclslActorAccessor.GetInt(host, MclslActorDataKeys.HuanzhenRestoreCount, 0) + 1;
            MclslActorAccessor.Set(host, MclslActorDataKeys.HuanzhenRestoreCount, count);
            MclslActorAccessor.Set(host, MclslActorDataKeys.HuanzhenAnchorYear, pending.AnchorYear);
            try
            {
                host.updateStats();
                float max = host.getMaxHealth();
                if (max > 0f) host.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, max));
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #2: " + mclslEmptyCatchEx.Message); }
        }
        finally { _applyingRestore = false; }

        _state.HostLastActorId = MclslActorAccessor.Id(host);
        _state.HostName = SafeName(host);
        _state.LastRestoreYear = MclslRuntime.CurrentYear();
        string originalPath = pending.OriginalSavePath;
        AddHistory(pending.DeathYear, pending.AnchorYear, pending.HostName, pending.Cultivation?.RealmId, pending.LoopDepth, "还真成功");
        pending.Active = false;
        _rollbackLoadInProgress = false;
        Flush();
        if (!string.IsNullOrWhiteSpace(originalPath))
        {
            try { SaveManager.setCurrentPath(originalPath); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #3: " + mclslEmptyCatchEx.Message); }
        }
        MclslWorldRunRepository.AddEvent(MclslRuntime.CurrentYear(), "huanzhen_return", MclslActorAccessor.DisplayName(host) + "还真归来", "其于" + pending.DeathYear + "年身死，因唯一还真之力回到" + pending.AnchorYear + "年，并保留死前境界与新法根基。连续避劫层数：" + pending.LoopDepth + "。", host);
        MclslWorldArchiveStore.SaveNow();
        MclslAnnouncementSystem.Enqueue(MclslActorAccessor.DisplayName(host) + "还真归来，死前修为尽数复归。", "#7CCFD0", 10f, 1);
    }


    internal static void AbortWorldLoadBinding()
    {
        _anyWorldLoadInProgress = false;
        if (_state.PendingRestore?.Active == true)
        {
            _state.PendingRestore.Active = false;
            _rollbackQueued = false;
            _rollbackLoadInProgress = false;
            AddHistory(_state.PendingRestore.DeathYear, _state.PendingRestore.AnchorYear, _state.PendingRestore.HostName, _state.PendingRestore.Cultivation?.RealmId, _state.PendingRestore.LoopDepth, "读档流程异常中止");
            Flush();
        }
    }

    internal static string StatusText()
    {
        EnsureLoaded();
        if (!MclslRuntimeSettings.HuanzhenEnabled) return "未降临";
        string currentRunId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(currentRunId) && !string.Equals(currentRunId, _state.WorldRunId, StringComparison.Ordinal)) return "本存档未绑定";
        if (string.IsNullOrWhiteSpace(_state.HostName)) return "未降临";
        string host = _state.HostName;
        string anchors = (_state.Anchors?.Count ?? 0).ToString();
        string latest = _state.LastAnchorYear < 0 ? "无" : _state.LastAnchorYear + "年";
        return host + "｜锚点" + anchors + "｜最近" + latest;
    }

    internal static void ClearRuntime()
    {
        _rollbackQueued = false;
        _rollbackDelayFrames = 0;
        // 回载锚点时 MapBox.clearWorld 会经过这里；必须保留“正在还真读档”状态，直到新世界 finishingUpLoading 完成。
        if (_state.PendingRestore?.Active != true) _rollbackLoadInProgress = false;
        _applyingRestore = false;
    }

    private static void BindHost(Actor actor, bool forceReset)
    {
        if (actor?.data == null) return;
        EnsureLoaded();
        string identity = EnsureIdentity(actor);
        string runId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        bool changed = forceReset && (!string.Equals(_state.HostIdentity, identity, StringComparison.Ordinal) || !string.Equals(_state.WorldRunId, runId, StringComparison.Ordinal));
        if (changed)
        {
            DeleteAnchorFolders(_state.Anchors);
            _state = new MclslHuanzhenExternalState
            {
                WorldRunId = runId,
                OriginalSavePath = SaveManager.currentSavePath ?? string.Empty,
                HostIdentity = identity,
                HostLastActorId = MclslActorAccessor.Id(actor),
                HostName = SafeName(actor)
            };
        }
        else
        {
            _state.WorldRunId = runId;
            _state.HostIdentity = identity;
            _state.HostLastActorId = MclslActorAccessor.Id(actor);
            _state.HostName = SafeName(actor);
            string currentPath = SaveManager.currentSavePath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(currentPath) && !currentPath.StartsWith("mclsl_huanzhen_", StringComparison.OrdinalIgnoreCase))
                _state.OriginalSavePath = currentPath;
        }
        Flush();
    }

    private static bool IsSafeToAnchor(Actor host, int year)
    {
        if (string.IsNullOrWhiteSpace(SaveManager.currentSavePath)) return false;
        if (_state.LastAnchorYear >= 0 && year - _state.LastAnchorYear < MclslRuntimeSettings.HuanzhenAnchorIntervalYears) return false;
        if (_state.LastRestoreYear >= 0 && year - _state.LastRestoreYear < MclslRuntimeSettings.HuanzhenSafetyGapYears) return false;
        try { if (host.isFighting()) return false; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #4: " + mclslEmptyCatchEx.Message); }
        try
        {
            float max = host.getMaxHealth();
            if (max > 0f && host.data.health / max < 0.85f) return false;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-5", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #5: " + mclslEmptyCatchEx.Message); }
        return true;
    }

    private static void TryCreateAnchor(Actor host, int year)
    {
        string runId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(runId)) return;
        string originalPath = SaveManager.currentSavePath ?? string.Empty;
        int sequence = ++_state.AnchorSequence;
        int ring = sequence % MaxAnchors;
        string relative = "mclsl_huanzhen_" + runId.Substring(0, Math.Min(10, runId.Length)) + "_" + ring;
        try
        {
            MclslActorAccessor.Set(host, MclslActorDataKeys.HuanzhenAnchorYear, year);
            MclslWorldArchiveStore.SaveNow();
            string full = SaveManager.folderPath(relative);
            if (!string.IsNullOrWhiteSpace(full) && Directory.Exists(full)) Directory.Delete(full, true);
            SaveManager.saveWorldToDirectory(relative, false, false);
            try { SaveManager.setCurrentPath(originalPath); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-6", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #6: " + mclslEmptyCatchEx.Message); }
            if (!SaveManager.doesSaveExist(relative)) return;
            _state.OriginalSavePath = originalPath;
            _state.LastAnchorYear = year;
            _state.Anchors.RemoveAll(x => string.Equals(x.RelativeSavePath, relative, StringComparison.OrdinalIgnoreCase));
            _state.Anchors.Add(new MclslHuanzhenAnchorRecord
            {
                RelativeSavePath = relative,
                Year = year,
                Sequence = sequence,
                HostIdentity = EnsureIdentity(host),
                HostName = SafeName(host),
                RealmIdAtAnchor = MclslActorAccessor.Realm(host)
            });
            _state.Anchors = _state.Anchors.OrderByDescending(x => x.Year).ThenByDescending(x => x.Sequence).Take(MaxAnchors).ToList();
            Flush();
            MclslWorldRunRepository.AddEvent(year, "huanzhen_anchor", MclslActorAccessor.DisplayName(host) + "定下还真锚点", "此锚点仅供唯一还真持有者死亡后回载；战斗中、重伤时与刚还真后的危险窗口不会建立新锚点。", host);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[模拟长生路][还真] 建立锚点失败: " + ex.Message);
            try { SaveManager.setCurrentPath(originalPath); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-7", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #7: " + mclslEmptyCatchEx.Message); }
        }
    }

    private static Actor FindLivingHost()
    {
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (units == null) return null;
        int checkedHosts = 0;
        for (int i = 0; i < units.Count && checkedHosts < 64; i++)
        {
            Actor actor = units[i];
            if (!HasHuanzhenTrait(actor)) continue;
            checkedHosts++;
            return actor;
        }
        return null;
    }

    private static Actor FindByIdentity(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity)) return null;
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (units == null) return null;
        for (int i = 0; i < units.Count; i++)
        {
            Actor actor = units[i];
            if (actor?.data == null) continue;
            if (string.Equals(MclslActorAccessor.GetString(actor, MclslActorDataKeys.HuanzhenIdentity, string.Empty), identity, StringComparison.Ordinal)) return actor;
        }
        return null;
    }

    private static string EnsureIdentity(Actor actor)
    {
        string identity = MclslActorAccessor.GetString(actor, MclslActorDataKeys.HuanzhenIdentity, string.Empty);
        if (!string.IsNullOrWhiteSpace(identity)) return identity;
        identity = Guid.NewGuid().ToString("N");
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HuanzhenIdentity, identity);
        return identity;
    }

    private static bool HasHuanzhenTrait(Actor actor)
    {
        try { return actor?.data != null && actor.hasTrait(MclslTraitRegistration.HuanzhenTraitId); }
        catch { return false; }
    }

    private static void DeleteAnchorFolders(IEnumerable<MclslHuanzhenAnchorRecord> anchors)
    {
        if (anchors == null) return;
        foreach (MclslHuanzhenAnchorRecord anchor in anchors)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.RelativeSavePath)) continue;
            try
            {
                string folder = SaveManager.folderPath(anchor.RelativeSavePath);
                if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder)) Directory.Delete(folder, true);
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-8", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #8: " + mclslEmptyCatchEx.Message); }
        }
    }

    private static MclslHuanzhenCultivationSnapshot CaptureCultivation(Actor a) => new()
    {
        CultivationSystemId = MclslActorAccessor.GetString(a, MclslActorDataKeys.CultivationSystem), RealmId = MclslActorAccessor.Realm(a), CultivationProgress = MclslActorAccessor.GetFloat(a, MclslActorDataKeys.CultivationProgress), TrueEssence = MclslActorAccessor.GetInt(a, MclslActorDataKeys.TrueEssence), Aptitude = MclslActorAccessor.GetInt(a, MclslActorDataKeys.Aptitude), MindState = MclslActorAccessor.GetInt(a, MclslActorDataKeys.MindState), HeartTemperingProgress = MclslActorAccessor.GetInt(a, MclslActorDataKeys.HeartTemperingProgress), HeartMethodKnown = MclslActorAccessor.GetInt(a, MclslActorDataKeys.HeartMethodKnown), MiasmaPoolCleansing = MclslActorAccessor.GetInt(a, MclslActorDataKeys.MiasmaPoolCleansing), Contribution = MclslActorAccessor.GetInt(a, MclslActorDataKeys.Contribution), RuinExperience = MclslActorAccessor.GetInt(a, MclslActorDataKeys.RuinExperience), TechniqueInsight = MclslActorAccessor.GetInt(a, MclslActorDataKeys.TechniqueInsight),
        TechniqueId = MclslActorAccessor.GetString(a, MclslActorDataKeys.TechniqueId), TechniqueName = MclslActorAccessor.GetString(a, MclslActorDataKeys.TechniqueName), TechniqueMaxRealm = MclslActorAccessor.GetString(a, MclslActorDataKeys.TechniqueMaxRealm), FoundationWonderId = MclslActorAccessor.GetString(a, MclslActorDataKeys.FoundationWonderId), FoundationWonderName = MclslActorAccessor.GetString(a, MclslActorDataKeys.FoundationWonderName), FoundationWonderQuality = MclslActorAccessor.GetInt(a, MclslActorDataKeys.FoundationWonderQuality), FoundationWonderTags = MclslActorAccessor.GetString(a, MclslActorDataKeys.FoundationWonderTags), FoundationWonderDescription = MclslActorAccessor.GetString(a, MclslActorDataKeys.FoundationWonderDescription), FoundationWonderOrigin = MclslActorAccessor.GetString(a, MclslActorDataKeys.FoundationWonderOrigin), FoundationWonderEffects = MclslActorAccessor.GetString(a, MclslActorDataKeys.FoundationWonderEffects),
        GoldenCoreLaws = MclslActorAccessor.GetString(a, MclslActorDataKeys.GoldenCoreLaws), GoldenCorePurity = MclslActorAccessor.GetInt(a, MclslActorDataKeys.GoldenCorePurity), GoldenCoreStability = MclslActorAccessor.GetInt(a, MclslActorDataKeys.GoldenCoreStability), NascentCaveId = MclslActorAccessor.GetString(a, MclslActorDataKeys.NascentCaveId), NascentCaveName = MclslActorAccessor.GetString(a, MclslActorDataKeys.NascentCaveName), NascentCaveTags = MclslActorAccessor.GetString(a, MclslActorDataKeys.NascentCaveTags), NascentCaveCompatibility = MclslActorAccessor.GetInt(a, MclslActorDataKeys.NascentCaveCompatibility), NascentCaveIntegrity = MclslActorAccessor.GetInt(a, MclslActorDataKeys.NascentCaveIntegrity),
        NascentEssenceId = MclslActorAccessor.GetString(a, MclslActorDataKeys.NascentEssenceId), NascentEssenceName = MclslActorAccessor.GetString(a, MclslActorDataKeys.NascentEssenceName), NascentEssenceQuality = MclslActorAccessor.GetInt(a, MclslActorDataKeys.NascentEssenceQuality), NascentEssenceTags = MclslActorAccessor.GetString(a, MclslActorDataKeys.NascentEssenceTags), NascentEssenceDescription = MclslActorAccessor.GetString(a, MclslActorDataKeys.NascentEssenceDescription), NascentEssenceEffects = MclslActorAccessor.GetString(a, MclslActorDataKeys.NascentEssenceEffects),
        DivineChangeId = MclslActorAccessor.GetString(a, MclslActorDataKeys.DivineChangeId), DivineChangeName = MclslActorAccessor.GetString(a, MclslActorDataKeys.DivineChangeName), DivineChangeTags = MclslActorAccessor.GetString(a, MclslActorDataKeys.DivineChangeTags), DivineChangeCompatibility = MclslActorAccessor.GetInt(a, MclslActorDataKeys.DivineChangeCompatibility), DivineMarrowId = MclslActorAccessor.GetString(a, MclslActorDataKeys.DivineMarrowId), DivineMarrowName = MclslActorAccessor.GetString(a, MclslActorDataKeys.DivineMarrowName), DivineMarrowCount = MclslActorAccessor.GetInt(a, MclslActorDataKeys.DivineMarrow), DivineMarrowQuality = MclslActorAccessor.GetInt(a, MclslActorDataKeys.DivineMarrowQuality), DivineMarrowTags = MclslActorAccessor.GetString(a, MclslActorDataKeys.DivineMarrowTags), DivineMarrowDescription = MclslActorAccessor.GetString(a, MclslActorDataKeys.DivineMarrowDescription), DivineMarrowEffects = MclslActorAccessor.GetString(a, MclslActorDataKeys.DivineMarrowEffects),
        HuaShenHonorific = MclslActorAccessor.GetString(a, MclslActorDataKeys.HuaShenHonorific), HeDaoHonorific = MclslActorAccessor.GetString(a, MclslActorDataKeys.HeDaoHonorific), ChangShengHonorific = MclslActorAccessor.GetString(a, MclslActorDataKeys.ChangShengHonorific),
        AncientHuaShenHonorific = MclslActorAccessor.GetString(a, MclslActorDataKeys.AncientHuaShenHonorific), AncientHeDaoHonorific = MclslActorAccessor.GetString(a, MclslActorDataKeys.AncientHeDaoHonorific), AncientChangShengHonorific = MclslActorAccessor.GetString(a, MclslActorDataKeys.AncientChangShengHonorific),
        WorldSoulId = MclslActorAccessor.GetString(a, MclslActorDataKeys.WorldSoulId), WorldSoulName = MclslActorAccessor.GetString(a, MclslActorDataKeys.WorldSoulName), HeavenlyDuty = MclslActorAccessor.GetString(a, MclslActorDataKeys.HeavenlyDuty), HarmonyLeap = MclslActorAccessor.GetInt(a, MclslActorDataKeys.HarmonyLeap), HarmonyOrigin = MclslActorAccessor.GetString(a, MclslActorDataKeys.HarmonyOrigin), HarmonyCompatibility = MclslActorAccessor.GetInt(a, MclslActorDataKeys.HarmonyCompatibility), HarmonyStability = MclslActorAccessor.GetInt(a, MclslActorDataKeys.HarmonyStability), InverseTruthId = MclslActorAccessor.GetString(a, MclslActorDataKeys.InverseTruthId), InverseTruthName = MclslActorAccessor.GetString(a, MclslActorDataKeys.InverseTruthName), InverseTruthProgress = MclslActorAccessor.GetInt(a, MclslActorDataKeys.InverseTruthProgress)
    };

    private static void AddHistory(int deathYear, int anchorYear, string name, string realm, int loopDepth, string result)
    {
        _state.History ??= new List<MclslHuanzhenHistoryRecord>();
        _state.History.Add(new MclslHuanzhenHistoryRecord { DeathYear = deathYear, AnchorYear = anchorYear, HostName = name ?? string.Empty, RealmId = realm ?? string.Empty, LoopDepth = loopDepth, Result = result ?? string.Empty });
        while (_state.History.Count > 80) _state.History.RemoveAt(0);
    }

    private static void Normalize()
    {
        _state ??= new MclslHuanzhenExternalState();
        _state.Version = MclslSaveVersions.HuanzhenExternalState;
        _state.Anchors ??= new List<MclslHuanzhenAnchorRecord>();
        _state.PendingRestore ??= new MclslHuanzhenPendingRestore();
        _state.History ??= new List<MclslHuanzhenHistoryRecord>();
    }

    private static void Flush()
    {
        EnsureLoaded();
        try
        {
            string dir = Path.GetDirectoryName(StatePath);
            if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(StatePath, JsonConvert.SerializeObject(_state, Formatting.Indented));
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路][还真] 外部状态写入失败: " + ex.Message); }
    }

    private static string SafeName(Actor actor) { try { return actor?.data == null ? "无名者" : MclslActorAccessor.DisplayName(actor); } catch { return "无名者"; } }
}
