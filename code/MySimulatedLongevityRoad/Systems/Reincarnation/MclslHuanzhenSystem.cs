using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Traits;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslHuanzhenSystem
{
    private const int MaxAnchors = 3;
    private const int MaxHistory = 40;
    private const int MaxLoadAttempts = 3;
    private const int MaxEssenceHistory = 2;
    private const int MaxLegacies = 8;
    private const int DefaultLegacyCarryLimit = 3;
    internal const int AnchorCost = 80;
    private static MclslHuanzhenExternalState _state = new();
    private static bool _loaded;
    private static bool _applyingRestore;
    private static bool _rollbackQueued;
    private static int _rollbackDelayFrames;
    private static bool _rollbackLoadInProgress;
    private static bool _anyWorldLoadInProgress;
    private static Actor _cachedHost;
    private static string _lastFlushedJson = string.Empty;
    private static bool _storageReconciled;
    private static bool _postLoadApplyQueued;
    private static int _postLoadApplyDelayFrames;
    private static int _hostResolveAttempts;
    private static int _lastAutomaticAnchorAttemptYear = -1;
    private static bool _grantingNaturalArrival;
    private static string StatePath => Path.Combine(Application.persistentDataPath, "MySimulatedLongevityRoad", "HuanzhenState.json");
    private static string AnchorStorageRoot => Path.Combine(Application.persistentDataPath, "MySimulatedLongevityRoad", "HuanzhenAnchors");

    internal static bool IsApplyingRestore => _applyingRestore;
    internal static bool IsRollbackLoadInProgress => _rollbackLoadInProgress;
    internal static MclslHuanzhenExternalState Current { get { EnsureLoaded(); return _state; } }

    internal static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            string tempPath = StatePath + ".tmp";
            if (!File.Exists(StatePath) && File.Exists(tempPath)) File.Move(tempPath, StatePath);
            if (File.Exists(StatePath))
            {
                string json = File.ReadAllText(StatePath);
                _state = JsonConvert.DeserializeObject<MclslHuanzhenExternalState>(json) ?? new();
                _lastFlushedJson = json;
            }
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路][还真] 外部状态读取失败: " + ex.Message); }
        Normalize();
    }

    internal static void Tick()
    {
        // 正常游玩时还真没有实时队列，直接退出，避免每帧重复进入存档状态机。
        // 世界载入/回溯排队时才继续执行下面的状态处理。
        if (_loaded && !_postLoadApplyQueued && !_rollbackQueued) return;
        EnsureLoaded();
        if (_postLoadApplyQueued)
        {
            if (_postLoadApplyDelayFrames-- > 0) return;
            _postLoadApplyQueued = false;
            OnWorldLoaded();
            return;
        }
        if (!_rollbackQueued || _rollbackLoadInProgress) return;
        if (_rollbackDelayFrames-- > 0) return;
        _rollbackQueued = false;
        MclslHuanzhenPendingRestore pending = _state.PendingRestore;
        if (pending == null || !pending.Active || string.IsNullOrWhiteSpace(pending.RelativeSavePath)) return;
        string anchorPath = ResolveAnchorSavePath(pending.RelativeSavePath);
        if (!SaveManager.doesSaveExist(anchorPath))
        {
            CancelPendingRestore(pending, "锚点存档不存在，取消还真");
            return;
        }
        if (pending.LoadAttempts >= MaxLoadAttempts)
        {
            CancelPendingRestore(pending, "连续读档失败达到上限，取消还真");
            return;
        }
        try
        {
            pending.LoadAttempts++;
            Flush();
            _rollbackLoadInProgress = true;
            MclslRuntime.ClearWorldState();
            if (World.world?.save_manager == null)
                throw new InvalidOperationException("SaveManager实例尚未就绪");
            SaveManager.setCurrentPath(anchorPath);
            World.world.save_manager.loadWorld(anchorPath, false);
        }
        catch (Exception ex)
        {
            _rollbackLoadInProgress = false;
            if (pending.LoadAttempts < MaxLoadAttempts && SaveManager.doesSaveExist(anchorPath))
            {
                _rollbackQueued = true;
                _rollbackDelayFrames = 30 * pending.LoadAttempts;
                Flush();
            }
            else
            {
                CancelPendingRestore(pending, "读档失败：" + ex.Message);
            }
            Debug.LogError("[模拟长生路][还真] 回载锚点失败: " + ex);
        }
    }

    internal static void TickAnnual(int year)
    {
        EnsureLoaded();
        Actor host = FindLivingHost();
        if (!MclslRuntimeSettings.HuanzhenEnabled || _rollbackLoadInProgress || _state.PendingRestore?.Active == true) return;
        if (host == null)
        {
            TryNaturalArrival(year);
            host = FindLivingHost();
            return;
        }
        EnforceUniqueHost(host);
        BindHost(host, false);
        PruneUnavailableAnchors();
        ReconcileAnchorStorageOnce();
        TryCreateAutomaticAnchor(year);
    }

    private static void TryCreateAutomaticAnchor(int year)
    {
        if (!MclslRuntimeSettings.AutoHuanzhenAnchor || CurrentSpaceEssence() < AnchorCost) return;
        if (_lastAutomaticAnchorAttemptYear == year) return;
        if (_state.LastAnchorYear >= 0 && year - _state.LastAnchorYear < MclslRuntimeSettings.HuanzhenAnchorIntervalYears) return;
        string replace = string.Empty;
        if ((_state.Anchors?.Count ?? 0) >= MaxAnchors)
        {
            MclslHuanzhenAnchorRecord oldest = _state.Anchors
                .Where(x => x != null)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Sequence)
                .FirstOrDefault();
            if (oldest == null) return;
            replace = oldest.RelativeSavePath;
        }
        _lastAutomaticAnchorAttemptYear = year;
        if (TryCreateManualAnchor(replace, out string message))
            MclslAnnouncementSystem.Enqueue("还真空间已自动锚定：" + message, "#7FAFB7", 7f, 1);
    }

    private static void TryNaturalArrival(int year)
    {
        // 仙道纪元只允许手动给予还真；自动寻主从新法纪元正式稳定后才开始。
        if (!MclslWorldEpochSystem.IsStableNewLawEra(year)) return;
        string runId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        if (year <= 0 || string.IsNullOrWhiteSpace(runId)) return;
        if (!string.Equals(_state.WorldRunId, runId, StringComparison.Ordinal))
        {
            DeleteAnchorFolders(_state.Anchors);
            _state = new MclslHuanzhenExternalState { WorldRunId = runId };
        }
        if (_state.NextNaturalArrivalYear < 0)
        {
            _state.NextNaturalArrivalYear = year + 5 + PositiveHash(runId + "|huanzhen-arrival") % 16;
            Flush();
            return;
        }
        if (year < _state.NextNaturalArrivalYear) return;

        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        Actor selected = null;
        int best = int.MaxValue;
        if (actors != null)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                Actor candidate = actors[i];
                if (!IsLivingActor(candidate) || HasHuanzhenTrait(candidate) || !MclslEligibility.CanCultivate(candidate)
                    || !MclslSpiritualRootSystem.HasActualSpiritualRoot(candidate)) continue;
                int rank = PositiveHash(runId + "|huanzhen-host|" + year + "|" + MclslActorAccessor.Id(candidate));
                if (rank >= best) continue;
                best = rank;
                selected = candidate;
            }
        }
        if (selected == null)
        {
            _state.NextNaturalArrivalYear = year + 5;
            Flush();
            return;
        }

        try
        {
            ActorTrait trait = AssetManager.traits.get(MclslTraitRegistration.HuanzhenTraitId);
            if (trait == null) throw new InvalidOperationException("还真特质尚未注册");
            _grantingNaturalArrival = true;
            try { selected.addTrait(trait, true); }
            finally { _grantingNaturalArrival = false; }
            BindHost(selected, true);
            _state.NaturalArrivalYear = year;
            _state.NextNaturalArrivalYear = -1;
            long previousEssence = CurrentSpaceEssence();
            _state.SpaceEssenceBase = Math.Max(20, previousEssence);
            _state.SpaceEssenceUpdatedYear = year;
            if (previousEssence < 20)
                AddEssenceHistory(year, "还真降临", "诸界倒影凝聚为初始灵蕴", 20 - previousEssence, _state.SpaceEssenceBase);
            Flush();
            string name = MclslActorAccessor.DisplayName(selected);
            MclslWorldRunRepository.AddEvent(year, "huanzhen_arrival", name + "偶得还真", "诸界倒影汇聚为还真空间，择“" + name + "”为此世唯一持有者。", selected);
            MclslAnnouncementSystem.Enqueue(name + "触及诸界倒影，成为此世唯一还真持有者。", "#69E6DD", 10f, 1);
        }
        catch (Exception ex)
        {
            _state.NextNaturalArrivalYear = year + 5;
            Flush();
            Debug.LogWarning("[模拟长生路][还真] 自然降临失败: " + ex.Message);
        }
    }

    internal static void OnTraitGranted(Actor actor)
    {
        if (_applyingRestore || actor?.data == null) return;
        EnsureLoaded();
        EnforceUniqueHost(actor);
        BindHost(actor, true);
        MclslTraitRegistration.TryAutoFavoriteHuanzhenHost(actor);
        if (_state.SpaceEssenceUpdatedYear < 0)
        {
            long previousEssence = CurrentSpaceEssence();
            _state.SpaceEssenceBase = Math.Max(20, previousEssence);
            _state.SpaceEssenceUpdatedYear = MclslRuntime.CurrentYear();
            if (previousEssence < 20)
                AddEssenceHistory(MclslRuntime.CurrentYear(), "还真绑定", "首次绑定宿主获得初始灵蕴", 20 - previousEssence, _state.SpaceEssenceBase);
            Flush();
        }
        string status = MclslRuntimeSettings.HuanzhenEnabled ? "还真已启用，可在还真空间中消耗80灵蕴手动建立锚点。" : "还真特质已绑定，但设置中的“启用还真”当前关闭。";
        string actorName = MclslActorAccessor.DisplayName(actor);
        if (!_grantingNaturalArrival)
            MclslWorldRunRepository.AddEvent(MclslRuntime.CurrentYear(), "huanzhen_grant", actorName + "获授还真", "玩家通过特质编辑器手动给予还真；原持有者的还真已被收回，以维持唯一性。", actor);
        MclslAnnouncementSystem.Enqueue(actorName + "成为唯一还真持有者。" + status, "#7CCFD0", 9f, 1);
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
        if (_state.PendingRestore?.Active == true || _rollbackLoadInProgress) return;
        string currentRunId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(currentRunId) || !string.Equals(currentRunId, _state.WorldRunId, StringComparison.Ordinal))
        {
            AddHistory(snapshot.DeathYear, -1, snapshot.ActorName, snapshot.Cultivation.RealmId, 0, "当前世界与还真绑定不一致");
            Flush();
            return;
        }
        List<MclslHuanzhenAnchorRecord> anchors = (_state.Anchors ?? new List<MclslHuanzhenAnchorRecord>())
            .Where(x => x != null && x.HostIdentity == snapshot.Identity && x.WorldRunId == currentRunId && x.Year <= snapshot.DeathYear && !string.IsNullOrWhiteSpace(x.RelativeSavePath))
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
            LoadAttempts = 0,
            Trigger = "death",
            HasSpaceEssenceBeforeRestore = true,
            SpaceEssenceBeforeRestore = CurrentSpaceEssence(),
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
        _cachedHost = null;
        _storageReconciled = false;
        ClearRuntime();
        Flush();
    }

    internal static void PrepareForAnyWorldLoad(string path)
    {
        EnsureLoaded();
        _anyWorldLoadInProgress = true;
        if (_state.PendingRestore?.Active == true && string.Equals(ResolveAnchorSavePath(_state.PendingRestore.RelativeSavePath), SaveManager.folderPath(path ?? string.Empty), StringComparison.OrdinalIgnoreCase))
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
        string anchorPath = ResolveAnchorSavePath(pending.RelativeSavePath);
        if (!string.Equals(SaveManager.folderPath(loadedPath), anchorPath, StringComparison.OrdinalIgnoreCase))
        {
            // 上次死亡后若游戏在真正回载前退出，PendingRestore 会保留；下一次进入任意世界时重新排队加载锚点，不能直接把死前修为注入错误存档。
            if (SaveManager.doesSaveExist(anchorPath))
            {
                if (pending.LoadAttempts >= MaxLoadAttempts)
                {
                    CancelPendingRestore(pending, "读档重试达到上限，取消还真");
                    return;
                }
                _rollbackLoadInProgress = false;
                _rollbackQueued = true;
                _rollbackDelayFrames = 2;
                return;
            }
            CancelPendingRestore(pending, "锚点存档不存在，取消还真");
            return;
        }

        Actor host = FindByIdentity(pending.HostIdentity);
        if (host == null)
        {
            _rollbackLoadInProgress = false;
            if (++_hostResolveAttempts < 18)
            {
                _postLoadApplyQueued = true;
                _postLoadApplyDelayFrames = 10;
                return;
            }
            _hostResolveAttempts = 0;
            CancelPendingRestore(pending, "锚点中未找到还真持有者");
            return;
        }
        _hostResolveAttempts = 0;
        try
        {
            _applyingRestore = true;
            if (!host.hasTrait(MclslTraitRegistration.HuanzhenTraitId))
            {
                ActorTrait huanzhen = AssetManager.traits.get(MclslTraitRegistration.HuanzhenTraitId);
                if (huanzhen != null) host.addTrait(huanzhen, true);
            }
            // The anchor save contains the old world state, but space essence is
            // external state. Preserve the value from the instant restore was
            // requested instead of accidentally reverting to the anchor-time value.
            if (pending.HasSpaceEssenceBeforeRestore)
            {
                _state.SpaceEssenceBase = Math.Max(0L, pending.SpaceEssenceBeforeRestore);
                _state.SpaceEssenceUpdatedYear = MclslRuntime.CurrentYear();
            }
            AddLegacy(pending);
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
        _cachedHost = host;
        _state.HostName = SafeName(host);
        _state.LastRestoreYear = MclslRuntime.CurrentYear();
        string originalPath = pending.OriginalSavePath;
        bool manualRestore = string.Equals(pending.Trigger, "manual", StringComparison.Ordinal);
        if (manualRestore)
        {
            _state.Anchors = (_state.Anchors ?? new List<MclslHuanzhenAnchorRecord>())
                .Where(x => x != null && x.Year <= pending.AnchorYear)
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Sequence).Take(MaxAnchors).ToList();
        }
        AddHistory(pending.DeathYear, pending.AnchorYear, pending.HostName, pending.Cultivation?.RealmId, pending.LoopDepth, manualRestore ? "手动还真成功" : "还真成功");
        pending.Active = false;
        _state.PendingRestore = new MclslHuanzhenPendingRestore();
        _rollbackLoadInProgress = false;
        Flush();
        if (!string.IsNullOrWhiteSpace(originalPath))
        {
            try { SaveManager.setCurrentPath(originalPath); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #3: " + mclslEmptyCatchEx.Message); }
        }
        string hostName = MclslActorAccessor.DisplayName(host);
        string eventType = manualRestore ? "huanzhen_manual_return" : "huanzhen_return";
        string eventTitle = hostName + (manualRestore ? "主动还真" : "还真归来");
        string eventDetail = manualRestore
            ? "其于" + pending.DeathYear + "年主动回到" + pending.AnchorYear + "年；回溯前的修为与所得已收入还真空间，可按当前锚点数量择取。"
            : "其于" + pending.DeathYear + "年身死，因唯一还真之力回到" + pending.AnchorYear + "年；死前遗产已收入还真空间，可按当前锚点数量择取。连续避劫层数：" + pending.LoopDepth + "。";
        MclslWorldRunRepository.AddEvent(MclslRuntime.CurrentYear(), eventType, eventTitle, eventDetail, host);
        MclslWorldArchiveStore.SaveNow();
        MclslAnnouncementSystem.Enqueue(hostName + (manualRestore ? "主动还真成功，回溯前所得已收入空间。" : "还真归来，前世遗产已收入空间，等待择取。"), "#7CCFD0", 10f, 1);
    }


    internal static void AbortWorldLoadBinding()
    {
        _anyWorldLoadInProgress = false;
        _postLoadApplyQueued = false;
        _hostResolveAttempts = 0;
        if (_state.PendingRestore?.Active == true)
        {
            _state.PendingRestore.Active = false;
            _rollbackQueued = false;
            _rollbackLoadInProgress = false;
            AddHistory(_state.PendingRestore.DeathYear, _state.PendingRestore.AnchorYear, _state.PendingRestore.HostName, _state.PendingRestore.Cultivation?.RealmId, _state.PendingRestore.LoopDepth, "读档流程异常中止");
            _state.PendingRestore = new MclslHuanzhenPendingRestore();
            Flush();
        }
    }

    internal static string StatusText()
    {
        EnsureLoaded();
        if (_state.PendingRestore?.Active == true)
            return "回载中｜第" + Math.Max(1, _state.PendingRestore.LoadAttempts) + "/" + MaxLoadAttempts + "次";
        if (!MclslRuntimeSettings.HuanzhenEnabled)
            return string.IsNullOrWhiteSpace(_state.HostName) ? "设置关闭" : "已绑定（设置关闭）";
        string currentRunId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(currentRunId) && !string.Equals(currentRunId, _state.WorldRunId, StringComparison.Ordinal)) return "本存档未绑定";
        if (FindLivingHost() == null)
            return _state.NextNaturalArrivalYear > MclslRuntime.CurrentYear()
                ? "寻主中｜约" + _state.NextNaturalArrivalYear + "年"
                : "等待降临";
        string host = _state.HostName;
        string anchors = (_state.Anchors?.Count ?? 0).ToString();
        string latest = _state.LastAnchorYear < 0 ? "无" : _state.LastAnchorYear + "年";
        return host + "｜锚点" + anchors + "｜最近" + latest;
    }

    internal static long CurrentSpaceEssence()
    {
        EnsureLoaded();
        return Math.Max(0L, _state.SpaceEssenceBase);
    }

    internal static bool SetSpaceEssenceForDeveloper(long value, out string message)
    {
        EnsureLoaded();
        if (!MclslDeveloperBridge.IsAvailable)
        {
            message = "开发者工具未开启。";
            return false;
        }
        _state.SpaceEssenceBase = Math.Max(0L, value);
        _state.SpaceEssenceUpdatedYear = MclslRuntime.CurrentYear();
        Flush();
        message = "空间灵蕴已修改为" + _state.SpaceEssenceBase + "。";
        return true;
    }

    internal static void OnHostPromotion(Actor actor, string previousRealm, string newRealm, int year, string reason)
    {
        EnsureLoaded();
        if (!MclslRuntimeSettings.HuanzhenEnabled || !IsCurrentHost(actor)) return;
        int previousIndex = MclslRealmIds.Index(previousRealm);
        int newIndex = MclslRealmIds.Index(newRealm);
        if (newIndex <= previousIndex) return;
        string cause = reason ?? string.Empty;
        if (cause.Contains("还真", StringComparison.Ordinal) || cause.Contains("玩家手动赋予", StringComparison.Ordinal)) return;
        int amount = newIndex switch
        {
            0 => 8,
            1 => 15,
            2 => 25,
            3 => 40,
            4 => 60,
            5 => 90,
            _ => 140
        };
        AddSpaceEssence(actor, amount, "宿主晋升", MclslRealmIds.Display(newRealm) + "突破", year, true);
    }

    internal static void ObserveHostKill(Actor victim, AttackType attackType)
    {
        if (victim?.data == null || !MclslDeathSystem.IsCombatDeath(attackType)) return;
        Actor killer = MclslNativeKillStatisticsSystem.ResolveKiller(victim);
        if (!IsCurrentHost(killer)) return;
        int victimRealm = MclslRealmIds.Index(MclslActorAccessor.Realm(victim));
        int amount = victimRealm switch
        {
            < 0 => 2,
            0 => 3,
            1 => 6,
            2 => 10,
            3 => 16,
            4 => 25,
            5 => 38,
            _ => 55
        };
        string target = SafeName(victim);
        string realm = victimRealm < 0 ? "凡俗" : MclslRealmIds.Display(MclslActorAccessor.Realm(victim));
        AddSpaceEssence(killer, amount, "杀人夺宝", "击杀" + realm + "·" + target, MclslRuntime.CurrentYear(), amount >= 25);
    }

    internal static void OnHostFortune(Actor actor, string source, int amount, string detail)
    {
        if (!IsCurrentHost(actor)) return;
        AddSpaceEssence(actor, Math.Max(0, amount), source, detail, MclslRuntime.CurrentYear(), amount >= 20);
    }

    private static void AddSpaceEssence(Actor host, long amount, string source, string detail, int year, bool announce)
    {
        if (amount <= 0 || !MclslRuntimeSettings.HuanzhenEnabled || !IsCurrentHost(host)) return;
        EnsureLoaded();
        long before = CurrentSpaceEssence();
        long balance = before > long.MaxValue - amount ? long.MaxValue : before + amount;
        long gained = balance - before;
        if (gained <= 0) return;
        _state.SpaceEssenceBase = balance;
        _state.SpaceEssenceUpdatedYear = Math.Max(0, year);
        AddEssenceHistory(year, source, detail, gained, balance);
        Flush();
        if (announce)
            MclslAnnouncementSystem.Enqueue(SafeName(host) + "因“" + source + "”获得空间灵蕴+" + gained + "。", "#69E6DD", 7f, 1);
    }

    private static void AddEssenceHistory(int year, string source, string detail, long amount, long balance)
    {
        _state.EssenceHistory ??= new List<MclslHuanzhenEssenceRecord>();
        _state.EssenceHistory.Add(new MclslHuanzhenEssenceRecord
        {
            Year = Math.Max(0, year),
            Source = source ?? string.Empty,
            Detail = detail ?? string.Empty,
            Amount = amount,
            Balance = Math.Max(0L, balance)
        });
        if (_state.EssenceHistory.Count > MaxEssenceHistory)
            _state.EssenceHistory.RemoveRange(0, _state.EssenceHistory.Count - MaxEssenceHistory);
    }

    private static void AddLegacy(MclslHuanzhenPendingRestore pending)
    {
        _state.Legacies ??= new List<MclslHuanzhenLegacyRecord>();
        string id = _state.WorldRunId + "|" + pending.HostIdentity + "|" + pending.DeathYear + "|" + pending.AnchorYear;
        _state.Legacies.RemoveAll(x => x == null || string.Equals(x.Id, id, StringComparison.Ordinal));
        _state.Legacies.Add(new MclslHuanzhenLegacyRecord
        {
            Id = id,
            DeathYear = pending.DeathYear,
            AnchorYear = pending.AnchorYear,
            HostName = pending.HostName,
            RealmId = pending.Cultivation?.RealmId ?? string.Empty,
            CarryLimit = DefaultLegacyCarryLimit,
            Snapshot = pending.Cultivation ?? new MclslHuanzhenCultivationSnapshot()
        });
        if (_state.Legacies.Count > MaxLegacies) _state.Legacies.RemoveRange(0, _state.Legacies.Count - MaxLegacies);
    }

    internal static int LegacySelectionLimit()
    {
        EnsureLoaded();
        return Math.Max(0, _state?.Anchors?.Count ?? 0);
    }

    internal static IReadOnlyList<MclslHuanzhenLegacyOption> GetLegacyOptions(MclslHuanzhenLegacyRecord legacy)
    {
        EnsureLoaded();
        List<MclslHuanzhenLegacyOption> options = new();
        MclslHuanzhenCultivationSnapshot s = legacy?.Snapshot;
        if (s == null) return options;
        AddLegacyOption(options, "cultivation", "修为根基：" + MclslRealmIds.Display(s.RealmId) + "·真元" + s.TrueEssence, !string.IsNullOrWhiteSpace(s.RealmId) || s.TrueEssence > 0);
        AddLegacyOption(options, "technique", "功法道统：" + Blank(s.TechniqueName, s.TechniqueId), !string.IsNullOrWhiteSpace(s.TechniqueName) || !string.IsNullOrWhiteSpace(s.TechniqueId));
        AddLegacyOption(options, "foundation_wonder", "突破造物·筑基奇物：" + Blank(s.FoundationWonderName, s.FoundationWonderId), !string.IsNullOrWhiteSpace(s.FoundationWonderName) || !string.IsNullOrWhiteSpace(s.FoundationWonderId));
        AddLegacyOption(options, "golden_core", "突破造物·金丹法则：" + s.GoldenCoreLaws, !string.IsNullOrWhiteSpace(s.GoldenCoreLaws));
        AddLegacyOption(options, "nascent_cave", "突破造物·元婴洞天：" + Blank(s.NascentCaveName, s.NascentCaveId), !string.IsNullOrWhiteSpace(s.NascentCaveName) || !string.IsNullOrWhiteSpace(s.NascentCaveId));
        AddLegacyOption(options, "nascent_essence", "天地道果·天地之精：" + Blank(s.NascentEssenceName, s.NascentEssenceId), !string.IsNullOrWhiteSpace(s.NascentEssenceName) || !string.IsNullOrWhiteSpace(s.NascentEssenceId));
        AddLegacyOption(options, "divine_change", "天地道果·神通变化：" + Blank(s.DivineChangeName, s.DivineChangeId), !string.IsNullOrWhiteSpace(s.DivineChangeName) || !string.IsNullOrWhiteSpace(s.DivineChangeId));
        AddLegacyOption(options, "divine_marrow", "天地道果·天地之髓：" + Blank(s.DivineMarrowName, s.DivineMarrowId) + "×" + s.DivineMarrowCount, !string.IsNullOrWhiteSpace(s.DivineMarrowName) || !string.IsNullOrWhiteSpace(s.DivineMarrowId) || s.DivineMarrowCount > 0);
        AddLegacyOption(options, "world_soul", "天地道果·天地之魄：" + Blank(s.WorldSoulName, s.WorldSoulId), !string.IsNullOrWhiteSpace(s.WorldSoulName) || !string.IsNullOrWhiteSpace(s.WorldSoulId));
        AddLegacyOption(options, "inverse_truth", "天地道果·逆理：" + Blank(s.InverseTruthName, s.InverseTruthId) + "·进度" + s.InverseTruthProgress + "%", !string.IsNullOrWhiteSpace(s.InverseTruthName) || !string.IsNullOrWhiteSpace(s.InverseTruthId));
        AddLegacyOption(options, "aptitude", "资质：" + s.Aptitude, s.Aptitude > 0);
        AddLegacyOption(options, "mind", "心境：心境" + s.MindState + "·淬心" + s.HeartTemperingProgress, s.MindState != 0 || s.HeartTemperingProgress != 0 || s.HeartMethodKnown != 0);
        AddLegacyOption(options, "contribution", "资源：贡献值" + s.Contribution, s.Contribution != 0);
        int stones = SnapshotInt(s, MclslActorDataKeys.SpiritStones);
        AddLegacyOption(options, "spirit_stones", "资源：灵石" + stones, stones != 0);
        if (s.TraitIds != null)
            foreach (string traitId in s.TraitIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal))
                AddLegacyOption(options, "trait:" + traitId, "资质/特征：" + TraitDisplayNameById(traitId), true);
        return options;
    }

    private static void AddLegacyOption(List<MclslHuanzhenLegacyOption> options, string id, string label, bool owned)
    {
        if (!owned || string.IsNullOrWhiteSpace(id)) return;
        options.Add(new MclslHuanzhenLegacyOption { Id = id, Category = id.StartsWith("trait:", StringComparison.Ordinal) ? "trait" : id, Label = label ?? id });
    }

    internal static bool TryClaimLegacyOption(string legacyId, string optionId, out string message)
    {
        EnsureLoaded();
        message = string.Empty;
        Actor host = FindLivingHost();
        MclslHuanzhenLegacyRecord legacy = FindLegacy(legacyId);
        if (host == null) { message = "未找到此世还真持有者。"; return false; }
        MclslHuanzhenLegacyOption option = GetLegacyOptions(legacy).FirstOrDefault(x => string.Equals(x.Id, optionId, StringComparison.Ordinal));
        if (option == null) { message = "前世档案中没有该具体遗产。"; return false; }
        if (option.Id.StartsWith("trait:", StringComparison.Ordinal)) return TryClaimLegacyTrait(legacyId, option.Id.Substring("trait:".Length), out message);
        if (!CanClaimLegacy(legacy, option.Id, out message)) return false;
        if (!MclslCultivationSystem.RestoreHuanzhenLegacyOption(host, legacy.Snapshot, option.Id, legacy.AnchorYear, legacy.DeathYear, out message)) return false;
        legacy.ClaimedChoices.Add(option.Id);
        Flush();
        MclslWorldArchiveStore.SaveNow();
        return true;
    }

    internal static bool TryReleaseLegacyOption(string legacyId, string optionId, out string message)
    {
        EnsureLoaded();
        MclslHuanzhenLegacyRecord legacy = FindLegacy(legacyId);
        if (legacy?.ClaimedChoices == null || !legacy.ClaimedChoices.Remove(optionId ?? string.Empty))
        {
            message = "该遗产尚未占用名额。";
            return false;
        }
        Flush();
        message = "已释放该前世遗产名额；已经写入人物的数据不会回退。";
        return true;
    }

    internal static bool TryClaimLegacyCategory(string legacyId, string category, out string message)
    {
        EnsureLoaded();
        message = string.Empty;
        Actor host = FindLivingHost();
        MclslHuanzhenLegacyRecord legacy = FindLegacy(legacyId);
        if (host == null) { message = "未找到此世还真持有者。"; return false; }
        if (legacy?.Snapshot == null) { message = "前世档案已经失效。"; return false; }
        string choice = "category:" + (category ?? string.Empty);
        if (!CanClaimLegacy(legacy, choice, out message)) return false;
        if (!MclslCultivationSystem.RestoreHuanzhenLegacyCategory(host, legacy.Snapshot, category, legacy.AnchorYear, legacy.DeathYear, out message)) return false;
        legacy.ClaimedChoices.Add(choice);
        Flush();
        MclslWorldArchiveStore.SaveNow();
        return true;
    }

    internal static bool TryClaimLegacyTrait(string legacyId, string traitId, out string message)
    {
        EnsureLoaded();
        message = string.Empty;
        Actor host = FindLivingHost();
        MclslHuanzhenLegacyRecord legacy = FindLegacy(legacyId);
        if (host == null) { message = "未找到此世还真持有者。"; return false; }
        if (legacy?.Snapshot?.TraitIds == null || !legacy.Snapshot.TraitIds.Contains(traitId)) { message = "前世档案中没有该特征。"; return false; }
        string choice = "trait:" + (traitId ?? string.Empty);
        if (!CanClaimLegacy(legacy, choice, out message)) return false;
        ActorTrait trait = AssetManager.traits.get(traitId);
        if (trait == null) { message = "当前环境缺少特征“" + traitId + "”。"; return false; }
        try
        {
            if (!host.hasTrait(traitId) && !host.addTrait(trait, true)) { message = "特征与当前人物冲突，未能继承。"; return false; }
        }
        catch (Exception ex) { message = "继承特征失败：" + ex.Message; return false; }
        legacy.ClaimedChoices.Add(choice);
        Flush();
        MclslWorldArchiveStore.SaveNow();
        message = "已继承前世特征：“" + TraitDisplayName(trait) + "”。";
        return true;
    }

    private static bool CanClaimLegacy(MclslHuanzhenLegacyRecord legacy, string choice, out string message)
    {
        legacy.ClaimedChoices ??= new List<string>();
        if (legacy.ClaimedChoices.Contains(choice)) { message = "这一项已经继承。"; return false; }
        int limit = LegacySelectionLimit();
        if (limit <= 0) { message = "当前没有还真锚点，不能保留前世遗产。"; return false; }
        if (legacy.ClaimedChoices.Count >= limit) { message = "本世可保留数量已用完（" + limit + "/" + limit + "）。"; return false; }
        message = string.Empty;
        return true;
    }

    private static MclslHuanzhenLegacyRecord FindLegacy(string id) =>
        (_state.Legacies ?? new List<MclslHuanzhenLegacyRecord>()).FirstOrDefault(x => x != null && string.Equals(x.Id, id, StringComparison.Ordinal));

    internal static string TraitDisplayNameById(string traitId)
    {
        try { return TraitDisplayName(AssetManager.traits.get(traitId)); }
        catch { return "未知特征"; }
    }

    private static string TraitDisplayName(ActorTrait trait)
    {
        if (trait == null) return "未知特征";
        try
        {
            string name = trait.getTranslatedName();
            return string.IsNullOrWhiteSpace(name) || string.Equals(name, trait.id, StringComparison.Ordinal)
            ? "未知特征"
                : name;
        }
        catch { return "未知特征"; }
    }

    private static string Blank(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static int SnapshotInt(MclslHuanzhenCultivationSnapshot snapshot, string key) =>
        snapshot?.IntState != null && snapshot.IntState.TryGetValue(key, out int value) ? value : 0;

    internal static void ClearRuntime()
    {
        _rollbackQueued = false;
        _rollbackDelayFrames = 0;
        _postLoadApplyQueued = false;
        _postLoadApplyDelayFrames = 0;
        _hostResolveAttempts = 0;
        _lastAutomaticAnchorAttemptYear = -1;
        // 回载锚点时 MapBox.clearWorld 会经过这里；必须保留“正在还真读档”状态，直到新世界 finishingUpLoading 完成。
        if (_state.PendingRestore?.Active != true) _rollbackLoadInProgress = false;
        _applyingRestore = false;
        _cachedHost = null;
    }

    private static void BindHost(Actor actor, bool forceReset)
    {
        if (actor?.data == null) return;
        EnsureLoaded();
        string identity = EnsureIdentity(actor);
        string runId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        bool changed = forceReset && (!string.Equals(_state.HostIdentity, identity, StringComparison.Ordinal) || !string.Equals(_state.WorldRunId, runId, StringComparison.Ordinal));
        bool dirty = changed;
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
            long actorId = MclslActorAccessor.Id(actor);
            string hostName = SafeName(actor);
            if (!string.Equals(_state.WorldRunId, runId, StringComparison.Ordinal)) { _state.WorldRunId = runId; dirty = true; }
            if (!string.Equals(_state.HostIdentity, identity, StringComparison.Ordinal)) { _state.HostIdentity = identity; dirty = true; }
            if (_state.HostLastActorId != actorId) { _state.HostLastActorId = actorId; dirty = true; }
            if (!string.Equals(_state.HostName, hostName, StringComparison.Ordinal)) { _state.HostName = hostName; dirty = true; }
            string currentPath = SaveManager.currentSavePath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(currentPath) && !IsAnchorSavePath(currentPath))
            {
                if (!string.Equals(_state.OriginalSavePath, currentPath, StringComparison.OrdinalIgnoreCase)) { _state.OriginalSavePath = currentPath; dirty = true; }
            }
        }
        _cachedHost = actor;
        MclslTraitRegistration.TryAutoFavoriteHuanzhenHost(actor);
        if (dirty) Flush();
    }

    private static bool CanCreateAnchor(Actor host, int year, out string reason)
    {
        reason = string.Empty;
        string currentPath = SaveManager.currentSavePath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(currentPath)) { reason = "请先将当前世界保存一次，再建立锚点。"; return false; }
        if (IsAnchorSavePath(currentPath)) { reason = "当前正处于还真内部存档，暂不能覆盖归途。"; return false; }
        if (_state.LastRestoreYear >= 0 && year - _state.LastRestoreYear < MclslRuntimeSettings.HuanzhenSafetyGapYears)
        {
            reason = "刚完成还真回溯，需等待至" + (_state.LastRestoreYear + MclslRuntimeSettings.HuanzhenSafetyGapYears) + "年后再锚定。";
            return false;
        }
        try { if (host.isFighting()) { reason = "宿主正在战斗，无法建立安全锚点。"; return false; } } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #4: " + mclslEmptyCatchEx.Message); }
        try
        {
            float max = host.getMaxHealth();
            if (max > 0f && host.data.health / max < 0.85f) { reason = "宿主伤势过重，生命恢复至85%以上才能锚定。"; return false; }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-5", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #5: " + mclslEmptyCatchEx.Message); }
        return true;
    }

    internal static bool TryCreateManualAnchor(string replaceRelativePath, out string message)
    {
        EnsureLoaded();
        message = string.Empty;
        if (!MclslRuntimeSettings.HuanzhenEnabled) { message = "设置中尚未启用还真。"; return false; }
        if (_state.PendingRestore?.Active == true || _rollbackLoadInProgress) { message = "还真正在回载，无法建立锚点。"; return false; }
        Actor host = FindLivingHost();
        if (host == null) { message = "此世尚无还真持有者。"; return false; }
        EnforceUniqueHost(host);
        BindHost(host, false);
        PruneUnavailableAnchors();
        ReconcileAnchorStorageOnce();
        int year = MclslRuntime.CurrentYear();
        if (!CanCreateAnchor(host, year, out message)) return false;
        long essence = CurrentSpaceEssence();
        if (essence < AnchorCost) { message = "空间灵蕴不足，建立锚点需要" + AnchorCost + "点，当前仅有" + essence + "点。"; return false; }

        string runId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(runId)) { message = "当前世界缺少运行标识，无法建立锚点。"; return false; }
        _state.Anchors ??= new List<MclslHuanzhenAnchorRecord>();
        MclslHuanzhenAnchorRecord replaced = null;
        if (!string.IsNullOrWhiteSpace(replaceRelativePath))
            replaced = _state.Anchors.FirstOrDefault(x => x != null && string.Equals(x.RelativeSavePath, replaceRelativePath, StringComparison.OrdinalIgnoreCase));
        if (_state.Anchors.Count >= MaxAnchors && replaced == null)
        {
            message = "三枚锚点已满，请先选择要替换的锚点。";
            return false;
        }
        if (!string.IsNullOrWhiteSpace(replaceRelativePath) && replaced == null)
        {
            message = "所选锚点已失效，请重新选择。";
            return false;
        }

        string originalPath = SaveManager.currentSavePath ?? string.Empty;
        int sequence = _state.AnchorSequence + 1;
        string relative = "mclsl_huanzhen_" + runId.Substring(0, Math.Min(10, runId.Length)) + "_" + sequence;
        string anchorPath = ResolveAnchorSavePath(relative);
        int priorAnchorYear = MclslActorAccessor.GetInt(host, MclslActorDataKeys.HuanzhenAnchorYear, -1);
        MclslHuanzhenCultivationSnapshot snapshot = CaptureCultivation(host);
        try
        {
            MclslActorAccessor.Set(host, MclslActorDataKeys.HuanzhenAnchorYear, year);
            MclslWorldArchiveStore.SaveNow();
            Directory.CreateDirectory(AnchorStorageRoot);
            SaveManager.saveWorldToDirectory(anchorPath, false, false);
            if (!SaveManager.doesSaveExist(anchorPath))
                throw new IOException("游戏未能在专用目录写出锚点存档：" + anchorPath);
            _state.AnchorSequence = sequence;
            _state.OriginalSavePath = originalPath;
            _state.LastAnchorYear = year;
            if (replaced != null) _state.Anchors.Remove(replaced);
            _state.Anchors.Add(new MclslHuanzhenAnchorRecord
            {
                RelativeSavePath = relative,
                WorldRunId = runId,
                Year = year,
                Sequence = sequence,
                HostIdentity = EnsureIdentity(host),
                HostName = SafeName(host),
                RealmIdAtAnchor = MclslActorAccessor.Realm(host),
                Snapshot = snapshot
            });
            _state.Anchors = _state.Anchors.OrderByDescending(x => x.Year).ThenByDescending(x => x.Sequence).Take(MaxAnchors).ToList();
            _state.SpaceEssenceBase = essence - AnchorCost;
            _state.SpaceEssenceUpdatedYear = year;
            AddEssenceHistory(year, replaced == null ? "建立锚点" : "替换锚点", "保存" + year + "年宿主状态", -AnchorCost, _state.SpaceEssenceBase);
            Flush();
            if (replaced != null) DeleteAnchorFolder(replaced.RelativeSavePath);
            MclslWorldRunRepository.AddEvent(year, "huanzhen_anchor", MclslActorAccessor.DisplayName(host) + "定下还真锚点", "此锚点仅供唯一还真持有者死亡后回载；战斗中、重伤时与刚还真后的危险窗口不会建立新锚点。", host);
            message = (replaced == null ? "已建立" : "已替换") + year + "年还真锚点，消耗空间灵蕴" + AnchorCost + "点；当前剩余" + _state.SpaceEssenceBase + "点。";
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[模拟长生路][还真] 建立锚点失败: " + ex.Message);
            try { SaveManager.setCurrentPath(originalPath); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-7", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #7: " + mclslEmptyCatchEx.Message); }
            MclslActorAccessor.Set(host, MclslActorDataKeys.HuanzhenAnchorYear, priorAnchorYear);
            DeleteAnchorFolder(relative);
            message = "建立锚点失败：" + ex.Message;
            return false;
        }
    }

    internal static bool TryManualRestore(string relativeSavePath, out string message)
    {
        EnsureLoaded();
        message = string.Empty;
        if (!MclslRuntimeSettings.HuanzhenEnabled) { message = "设置中尚未启用还真。"; return false; }
        if (_state.PendingRestore?.Active == true || _rollbackLoadInProgress || _rollbackQueued) { message = "还真正在回载，请勿重复发动。"; return false; }
        Actor host = FindLivingHost();
        if (host == null) { message = "此世尚无存活的还真宿主。"; return false; }
        EnforceUniqueHost(host);
        BindHost(host, false);
        PruneUnavailableAnchors();
        string runId = MclslWorldRunRepository.Current?.RunId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(runId) || !string.Equals(runId, _state.WorldRunId, StringComparison.Ordinal))
        { message = "当前世界与还真空间的绑定不一致，无法回溯。"; return false; }
        string identity = EnsureIdentity(host);
        MclslHuanzhenAnchorRecord selected = (_state.Anchors ?? new List<MclslHuanzhenAnchorRecord>())
            .FirstOrDefault(x => x != null
                && string.Equals(x.RelativeSavePath, relativeSavePath, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.WorldRunId, runId, StringComparison.Ordinal)
                && string.Equals(x.HostIdentity, identity, StringComparison.Ordinal));
        if (selected == null) { message = "所选锚点已失效，或不属于当前世界与宿主。"; return false; }
        int year = MclslRuntime.CurrentYear();
        if (selected.Year > year) { message = "所选锚点来自当前年份之后，无法回溯。"; return false; }
        string anchorPath = ResolveAnchorSavePath(selected.RelativeSavePath);
        if (string.IsNullOrWhiteSpace(anchorPath) || !SaveManager.doesSaveExist(anchorPath))
        { message = "所选锚点存档不存在，请重新建立锚点。"; return false; }
        string originalPath = SaveManager.currentSavePath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(originalPath)) { message = "请先保存当前世界，再发动手动还真。"; return false; }
        if (IsAnchorSavePath(originalPath)) { message = "当前正处于还真内部存档，不能再次回溯。"; return false; }
        _state.OriginalSavePath = originalPath;
        _state.PendingRestore = new MclslHuanzhenPendingRestore
        {
            Active = true,
            RelativeSavePath = selected.RelativeSavePath,
            OriginalSavePath = originalPath,
            WorldRunId = runId,
            HostIdentity = identity,
            HostName = SafeName(host),
            AnchorYear = selected.Year,
            DeathYear = year,
            LoopDepth = 0,
            LoadAttempts = 0,
            Trigger = "manual",
            HasSpaceEssenceBeforeRestore = true,
            SpaceEssenceBeforeRestore = CurrentSpaceEssence(),
            Cultivation = CaptureCultivation(host)
        };
        Flush();
        _rollbackQueued = true;
        _rollbackDelayFrames = 2;
        message = "手动还真已发动：将由" + year + "年回到" + selected.Year + "年；本次不消耗空间灵蕴。";
        MclslAnnouncementSystem.Enqueue(message, "#7CCFD0", 10f, 1);
        return true;
    }

    private static bool IsCurrentHost(Actor actor)
    {
        if (!IsLivingActor(actor) || !HasHuanzhenTrait(actor)) return false;
        EnsureLoaded();
        string identity = EnsureIdentity(actor);
        long actorId = MclslActorAccessor.Id(actor);
        return (!string.IsNullOrWhiteSpace(_state.HostIdentity) && string.Equals(_state.HostIdentity, identity, StringComparison.Ordinal))
            || (_state.HostLastActorId > 0L && _state.HostLastActorId == actorId);
    }

    private static Actor FindLivingHost()
    {
        if (IsLivingActor(_cachedHost) && HasHuanzhenTrait(_cachedHost)) return _cachedHost;
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (units == null) return null;
        int checkedHosts = 0;
        for (int i = 0; i < units.Count && checkedHosts < 64; i++)
        {
            Actor actor = units[i];
            if (!IsLivingActor(actor) || !HasHuanzhenTrait(actor)) continue;
            checkedHosts++;
            _cachedHost = actor;
            return actor;
        }
        return null;
    }

    private static Actor FindByIdentity(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity)) return null;
        if (_cachedHost?.data != null && string.Equals(MclslActorAccessor.GetString(_cachedHost, MclslActorDataKeys.HuanzhenIdentity, string.Empty), identity, StringComparison.Ordinal)) return _cachedHost;
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (units == null) return null;
        for (int i = 0; i < units.Count; i++)
        {
            Actor actor = units[i];
            if (actor?.data == null) continue;
            if (string.Equals(MclslActorAccessor.GetString(actor, MclslActorDataKeys.HuanzhenIdentity, string.Empty), identity, StringComparison.Ordinal)) { _cachedHost = actor; return actor; }
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

    private static bool IsLivingActor(Actor actor)
    {
        if (actor?.data == null) return false;
        try { return actor.isAlive(); }
        catch { return false; }
    }

    private static void EnforceUniqueHost(Actor primary)
    {
        if (_cachedHost != null && _cachedHost != primary && HasHuanzhenTrait(_cachedHost))
        {
            try { _cachedHost.removeTrait(MclslTraitRegistration.HuanzhenTraitId); }
            catch (Exception ex) { MclslDiagnostics.Error("huanzhen-unique-cached-host", "移除缓存中的重复还真持有者失败: " + ex.Message); }
        }
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (units == null) return;
        for (int i = 0; i < units.Count; i++)
        {
            Actor other = units[i];
            if (other == null || other == primary || other.data == null || !HasHuanzhenTrait(other)) continue;
            try { other.removeTrait(MclslTraitRegistration.HuanzhenTraitId); }
            catch (Exception ex) { MclslDiagnostics.Error("huanzhen-unique-host", "移除重复还真持有者失败: " + ex.Message); }
        }
        _cachedHost = primary;
    }

    private static void DeleteAnchorFolders(IEnumerable<MclslHuanzhenAnchorRecord> anchors)
    {
        if (anchors == null) return;
        foreach (MclslHuanzhenAnchorRecord anchor in anchors)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.RelativeSavePath)) continue;
            try
            {
                string folder = ResolveAnchorSavePath(anchor.RelativeSavePath);
                if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder)) Directory.Delete(folder, true);
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Reincarnation-MclslHuanzhenSystem-cs-8", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Reincarnation/MclslHuanzhenSystem.cs #8: " + mclslEmptyCatchEx.Message); }
        }
    }

    private static void DeleteAnchorFolder(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;
        try
        {
            string folder = ResolveAnchorSavePath(relativePath);
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder)) Directory.Delete(folder, true);
        }
        catch (Exception ex) { MclslDiagnostics.Error("huanzhen-anchor-cleanup", "清理无效还真锚点失败: " + ex.Message); }
    }

    private static void PruneUnavailableAnchors()
    {
        if (_state.Anchors == null || _state.Anchors.Count == 0) return;
        int removed = _state.Anchors.RemoveAll(x => x == null
            || string.IsNullOrWhiteSpace(x.RelativeSavePath)
            || !string.Equals(x.HostIdentity, _state.HostIdentity, StringComparison.Ordinal)
            || !string.Equals(x.WorldRunId, _state.WorldRunId, StringComparison.Ordinal)
            || !SaveManager.doesSaveExist(ResolveAnchorSavePath(x.RelativeSavePath)));
        if (removed <= 0) return;
        _state.LastAnchorYear = _state.Anchors.Count == 0 ? -1 : _state.Anchors.Max(x => x.Year);
        Flush();
    }

    private static void ReconcileAnchorStorageOnce()
    {
        if (_storageReconciled) return;
        _storageReconciled = true;
        try
        {
            string root = Path.GetFullPath(AnchorStorageRoot);
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
            HashSet<string> retained = new((_state.Anchors ?? new List<MclslHuanzhenAnchorRecord>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.RelativeSavePath))
                .Select(x => Path.GetFullPath(ResolveAnchorSavePath(x.RelativeSavePath))), StringComparer.OrdinalIgnoreCase);
            if (_state.PendingRestore?.Active == true && !string.IsNullOrWhiteSpace(_state.PendingRestore.RelativeSavePath))
                retained.Add(Path.GetFullPath(ResolveAnchorSavePath(_state.PendingRestore.RelativeSavePath)));
            foreach (string folder in Directory.GetDirectories(root, "mclsl_huanzhen_*", SearchOption.TopDirectoryOnly))
            {
                string full = Path.GetFullPath(folder);
                if (retained.Contains(full)) continue;
                // Only delete direct children with the mod-owned prefix.
                if (!string.Equals(Path.GetDirectoryName(full), root, StringComparison.OrdinalIgnoreCase)) continue;
                Directory.Delete(full, true);
            }
        }
        catch (Exception ex) { MclslDiagnostics.Error("huanzhen-storage-reconcile", "整理还真锚点目录失败: " + ex.Message); }
    }

    private static string ResolveAnchorSavePath(string storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath)) return string.Empty;
        // SaveManager 接受的是实际文件夹路径，不会把相对名称自动拼到游戏存档目录。
        string name = Path.GetFileName(storedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(name) || !name.StartsWith("mclsl_huanzhen_", StringComparison.OrdinalIgnoreCase))
            return string.Empty;
        return SaveManager.folderPath(Path.Combine(AnchorStorageRoot, name));
    }

    private static bool IsAnchorSavePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        string resolved = SaveManager.folderPath(Path.GetFullPath(path));
        string root = SaveManager.folderPath(Path.GetFullPath(AnchorStorageRoot));
        return resolved.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    private static void CancelPendingRestore(MclslHuanzhenPendingRestore pending, string reason)
    {
        pending.Active = false;
        _rollbackQueued = false;
        _rollbackLoadInProgress = false;
        AddHistory(pending.DeathYear, pending.AnchorYear, pending.HostName, pending.Cultivation?.RealmId, pending.LoopDepth, reason);
        _state.PendingRestore = new MclslHuanzhenPendingRestore();
        Flush();
        Debug.LogWarning("[模拟长生路][还真] " + reason);
    }

    private static MclslHuanzhenCultivationSnapshot CaptureCultivation(Actor a)
    {
        MclslHuanzhenCultivationSnapshot snapshot = new()
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
        CaptureSupplementalState(a, snapshot);
        return snapshot;
    }

    private static readonly string[] SupplementalStringKeys =
    {
        MclslActorDataKeys.SpiritualRootPrimary, MclslActorDataKeys.SpiritualRootAttributes,
        MclslActorDataKeys.FactionAffiliation, MclslActorDataKeys.AncientLawStatus,
        MclslActorDataKeys.AncientFoundationName, MclslActorDataKeys.AncientCoreName,
        MclslActorDataKeys.AncientDaoIntent, MclslActorDataKeys.AncientNascentName,
        MclslActorDataKeys.AncientDaoName
    };

    private static readonly string[] SupplementalIntKeys =
    {
        MclslActorDataKeys.ImmortalFate, MclslActorDataKeys.SpiritualRootCount,
        MclslActorDataKeys.MortalMiasma, MclslActorDataKeys.SpiritStones,
        MclslActorDataKeys.FoundationChanceBonus, MclslActorDataKeys.CaveClaimBonus,
        MclslActorDataKeys.DivineClaimBonus, MclslActorDataKeys.FoundationWonderGrade,
        MclslActorDataKeys.FoundationWonderCompleteness, MclslActorDataKeys.FoundationWonderRuleStrength,
        MclslActorDataKeys.AncientLineageStrength, MclslActorDataKeys.AncientLegacyPotential,
        MclslActorDataKeys.AncientFoundationQuality, MclslActorDataKeys.AncientFoundationStability,
        MclslActorDataKeys.AncientCorePurity, MclslActorDataKeys.AncientSoulStrength,
        MclslActorDataKeys.AncientBodyFit, MclslActorDataKeys.AncientDivineIntent,
        MclslActorDataKeys.AncientSoulFusion, MclslActorDataKeys.AncientDaoCompatibility,
        MclslActorDataKeys.AncientTechniqueComprehension, MclslActorDataKeys.AncientHarmonyIntegrity,
        MclslActorDataKeys.AncientHeavenCompatibility, MclslActorDataKeys.TaishangProgress,
        MclslActorDataKeys.LifespanStolenBonus, MclslActorDataKeys.LifespanDrainedPenalty,
        MclslActorDataKeys.HeavenlyDutyBacklash
    };

    private static readonly string[] SupplementalFloatKeys = { MclslActorDataKeys.TrueEssenceRemainder };

    private static void CaptureSupplementalState(Actor actor, MclslHuanzhenCultivationSnapshot snapshot)
    {
        snapshot.StringState ??= new Dictionary<string, string>();
        snapshot.IntState ??= new Dictionary<string, int>();
        snapshot.FloatState ??= new Dictionary<string, float>();
        foreach (string key in SupplementalStringKeys) snapshot.StringState[key] = MclslActorAccessor.GetString(actor, key, string.Empty);
        foreach (string key in SupplementalIntKeys) snapshot.IntState[key] = MclslActorAccessor.GetInt(actor, key, 0);
        foreach (string key in SupplementalFloatKeys) snapshot.FloatState[key] = MclslActorAccessor.GetFloat(actor, key, 0f);
        snapshot.TraitIds ??= new List<string>();
        snapshot.TraitIds.Clear();
        try
        {
            foreach (ActorTrait trait in actor.traits)
            {
                string id = trait?.id ?? string.Empty;
                if (string.IsNullOrWhiteSpace(id) || IsExcludedLegacyTrait(id)) continue;
                if (!snapshot.TraitIds.Contains(id)) snapshot.TraitIds.Add(id);
            }
            snapshot.TraitIds.Sort(StringComparer.Ordinal);
        }
        catch (Exception ex) { MclslDiagnostics.Error("huanzhen-trait-snapshot", "捕获前世特征失败: " + ex.Message); }
    }

    private static bool IsExcludedLegacyTrait(string id)
    {
        if (string.Equals(id, MclslTraitRegistration.HuanzhenTraitId, StringComparison.Ordinal)
            || string.Equals(id, MclslTraitRegistration.WorldSoulEntityTraitId, StringComparison.Ordinal)) return true;
        foreach (string realm in MclslRealmIds.Ordered)
        {
            if (string.Equals(id, MclslTraitRegistration.TraitIdForRealm(realm), StringComparison.Ordinal)
                || string.Equals(id, MclslTraitRegistration.LegacyTraitIdForRealm(realm), StringComparison.Ordinal)) return true;
        }
        return false;
    }

    internal static void RestoreSupplementalState(Actor actor, MclslHuanzhenCultivationSnapshot snapshot)
    {
        if (actor?.data == null || snapshot == null) return;
        foreach (string key in SupplementalStringKeys)
            if (snapshot.StringState?.TryGetValue(key, out string text) == true) MclslActorAccessor.Set(actor, key, text ?? string.Empty);
        foreach (string key in SupplementalIntKeys)
            if (snapshot.IntState?.TryGetValue(key, out int number) == true) MclslActorAccessor.Set(actor, key, number);
        foreach (string key in SupplementalFloatKeys)
            if (snapshot.FloatState?.TryGetValue(key, out float number) == true) MclslActorAccessor.Set(actor, key, number);
    }

    private static void AddHistory(int deathYear, int anchorYear, string name, string realm, int loopDepth, string result)
    {
        _state.History ??= new List<MclslHuanzhenHistoryRecord>();
        _state.EssenceHistory ??= new List<MclslHuanzhenEssenceRecord>();
        _state.Legacies ??= new List<MclslHuanzhenLegacyRecord>();
        _state.History.Add(new MclslHuanzhenHistoryRecord { DeathYear = deathYear, AnchorYear = anchorYear, HostName = name ?? string.Empty, RealmId = realm ?? string.Empty, LoopDepth = loopDepth, Result = result ?? string.Empty });
        if (_state.History.Count > MaxHistory)
            _state.History.RemoveRange(0, _state.History.Count - MaxHistory);
    }

    private static void Normalize()
    {
        _state ??= new MclslHuanzhenExternalState();
        _state.Version = MclslSaveVersions.HuanzhenExternalState;
        _state.Anchors ??= new List<MclslHuanzhenAnchorRecord>();
        _state.PendingRestore ??= new MclslHuanzhenPendingRestore();
        _state.History ??= new List<MclslHuanzhenHistoryRecord>();
        _state.EssenceHistory ??= new List<MclslHuanzhenEssenceRecord>();
        _state.Legacies ??= new List<MclslHuanzhenLegacyRecord>();
        // v0.1.8 used a 999-point cap. Keep only the invalid-negative guard;
        // v0.1.9 deliberately has no gameplay upper limit.
        _state.SpaceEssenceBase = Math.Max(0L, _state.SpaceEssenceBase);
        _state.Anchors.RemoveAll(x => x == null || string.IsNullOrWhiteSpace(x.RelativeSavePath));
        foreach (MclslHuanzhenAnchorRecord anchor in _state.Anchors)
        {
            if (string.IsNullOrWhiteSpace(anchor.WorldRunId)) anchor.WorldRunId = _state.WorldRunId;
            anchor.Snapshot ??= new MclslHuanzhenCultivationSnapshot();
            NormalizeSnapshot(anchor.Snapshot);
        }
        _state.Anchors = _state.Anchors.OrderByDescending(x => x.Year).ThenByDescending(x => x.Sequence).Take(MaxAnchors).ToList();
        if (_state.History.Count > MaxHistory) _state.History.RemoveRange(0, _state.History.Count - MaxHistory);
        if (_state.EssenceHistory.Count > MaxEssenceHistory) _state.EssenceHistory.RemoveRange(0, _state.EssenceHistory.Count - MaxEssenceHistory);
        _state.Legacies.RemoveAll(x => x == null || x.Snapshot == null || string.IsNullOrWhiteSpace(x.Id));
        if (_state.Legacies.Count > MaxLegacies) _state.Legacies.RemoveRange(0, _state.Legacies.Count - MaxLegacies);
        foreach (MclslHuanzhenLegacyRecord legacy in _state.Legacies)
        {
            legacy.ClaimedChoices ??= new List<string>();
            NormalizeSnapshot(legacy.Snapshot);
            legacy.CarryLimit = Math.Clamp(legacy.CarryLimit <= 0 ? DefaultLegacyCarryLimit : legacy.CarryLimit, 1, 8);
        }
        if (string.IsNullOrWhiteSpace(_state.PendingRestore.Trigger)) _state.PendingRestore.Trigger = "death";
        _state.PendingRestore.Cultivation ??= new MclslHuanzhenCultivationSnapshot();
        NormalizeSnapshot(_state.PendingRestore.Cultivation);
    }

    private static void NormalizeSnapshot(MclslHuanzhenCultivationSnapshot snapshot)
    {
        if (snapshot == null) return;
        snapshot.StringState ??= new Dictionary<string, string>();
        snapshot.IntState ??= new Dictionary<string, int>();
        snapshot.FloatState ??= new Dictionary<string, float>();
        snapshot.TraitIds ??= new List<string>();
    }

    private static void Flush()
    {
        EnsureLoaded();
        try
        {
            string json = JsonConvert.SerializeObject(_state, Formatting.None);
            if (string.Equals(json, _lastFlushedJson, StringComparison.Ordinal)) return;
            string dir = Path.GetDirectoryName(StatePath);
            if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
            string tempPath = StatePath + ".tmp";
            File.WriteAllText(tempPath, json);
            if (File.Exists(StatePath))
            {
                try { File.Replace(tempPath, StatePath, null); }
                catch (PlatformNotSupportedException) { File.Copy(tempPath, StatePath, true); File.Delete(tempPath); }
                catch (IOException) { File.Copy(tempPath, StatePath, true); File.Delete(tempPath); }
            }
            else File.Move(tempPath, StatePath);
            _lastFlushedJson = json;
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路][还真] 外部状态写入失败: " + ex.Message); }
    }

    private static string SafeName(Actor actor) { try { return actor?.data == null ? "无名者" : MclslActorAccessor.DisplayName(actor); } catch { return "无名者"; } }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 23;
            string text = value ?? string.Empty;
            for (int i = 0; i < text.Length; i++) hash = hash * 31 + text[i];
            return hash & int.MaxValue;
        }
    }
}
