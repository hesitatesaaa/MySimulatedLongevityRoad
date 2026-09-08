using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow : MonoBehaviour
{
    private static MclslCodexWindow _instance;
    private bool _visible;
    private bool _pauseCaptured;
    private float _savedTimeScale = 1f;
    private bool _savedConfigPaused;
    private int _tab;
    private string _eventCategory = MclslEventCatalog.All;
    private string _ancientEventFilter = MclslEventCatalog.All;
    private string _ancientTeachingMaxRealmFilter = MclslEventCatalog.All;
    private string _ancientBreakthroughRealmFilter = MclslEventCatalog.All;
    private string _ancientMindRealmFilter = MclslEventCatalog.All;
    private Vector2 _scroll;
    private string _deathRealmFilter = MclslEventCatalog.All;
    private int _ruinViewMode;
    private string _kingdomDetailName = string.Empty;
    private string _kingdomRealmFilter = MclslEventCatalog.All;
    private Rect _rect = new(60f, 60f, 1600f, 1230f);
    private MclslCodexSnapshot _snapshot = new();
    private static bool _stylesReady;
    private static GUIStyle _windowStyle;
    private static GUIStyle _buttonStyle;
    private static GUIStyle _labelStyle;
    private static GUIStyle _tagStyle;
    private static GUIStyle _oldLabel;
    private static GUIStyle _oldButton;
    private static GUIStyle _oldWindow;
    private static Texture2D _windowBackground;
    private static Texture2D _backdropTexture;
    private static Texture2D _whiteTexture;
    private static GameObject _overlayBlocker;
    private static readonly Dictionary<string, Texture2D> IconCache = new(StringComparer.Ordinal);
    internal static void Show()
    {
        if (_instance == null)
        {
            GameObject host = new("MclslCodexWindow");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<MclslCodexWindow>();
        }
        MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        _instance._snapshot = MclslCodexSnapshot.Build();
        _instance._rect = FitRect();
        _instance._visible = true;
        _instance.enabled = true;
        CreateOverlayBlocker();
        _instance.ApplyCodexPause();
    }

    private void Update()
    {
        if (_visible)
        {
            EnforceCodexPause();
            if (Input.GetKeyDown(KeyCode.Escape)) CloseWindow();
        }
        else DestroyOverlayBlocker();
    }

    private void OnGUI()
    {
        if (!_visible) return;
        EnsureStyles();
        CreateOverlayBlocker();
        DrawBackdrop();
        Color oldColor = GUI.color;
        Color oldBackground = GUI.backgroundColor;
        try
        {
            _oldWindow = GUI.skin.window;
            GUI.skin.window = _windowStyle;
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            _rect = GUI.Window(781203, _rect, DrawWindow, "玄黄仙录");
        }
        finally
        {
            GUI.skin.window = _oldWindow;
            GUI.color = oldColor;
            GUI.backgroundColor = oldBackground;
        }
    }

    private void DrawWindow(int id)
    {
        _oldLabel = GUI.skin.label;
        _oldButton = GUI.skin.button;
        try
        {
            GUI.skin.label = _labelStyle;
            GUI.skin.button = _buttonStyle;
            GUILayout.Space(18f);
            DrawTabRow();
            GUILayout.Space(9f);
            _scroll = GUILayout.BeginScrollView(_scroll);
            DrawPage();
            GUILayout.EndScrollView();
            GUI.DragWindow();
        }
        finally
        {
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            GUI.skin.label = _oldLabel;
            GUI.skin.button = _oldButton;
        }
    }

    private void DrawTabRow()
    {
        MclslCodexTab[] tabs = ActiveTabs();
        if (_tab >= tabs.Length) _tab = 0;
        GUILayout.BeginHorizontal();
        for (int i = 0; i < tabs.Length; i++)
        {
            GUI.backgroundColor = _tab == i ? new Color(0.3f, 0.3f, 0.3f) : Color.gray;
            if (GUILayout.Button(tabs[i].Title, GUILayout.Height(42f)))
            {
                _tab = i;
                _scroll = Vector2.zero;
                _kingdomDetailName = string.Empty;
                _kingdomRealmFilter = MclslEventCatalog.All;
                _ancientEventFilter = MclslEventCatalog.All;
                _ancientTeachingMaxRealmFilter = MclslEventCatalog.All;
            }
        }
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("×", GUILayout.Width(45f), GUILayout.Height(42f))) CloseWindow();
        GUILayout.EndHorizontal();
    }

    private void CloseWindow()
    {
        _visible = false;
        DestroyOverlayBlocker();
        ReleaseCodexPause();
    }

    private void ApplyCodexPause()
    {
        if (_pauseCaptured)
        {
            EnforceCodexPause();
            return;
        }
        _savedTimeScale = Time.timeScale;
        _savedConfigPaused = Config.paused;
        _pauseCaptured = true;
        EnforceCodexPause();
    }

    private static void EnforceCodexPause()
    {
        Config.paused = true;
        Time.timeScale = 0f;
    }

    private void ReleaseCodexPause()
    {
        if (!_pauseCaptured) return;
        Config.paused = _savedConfigPaused;
        Time.timeScale = _savedTimeScale < 0f ? 1f : _savedTimeScale;
        _pauseCaptured = false;
    }

    private void DrawPage()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current ?? new MclslWorldRunState();
        if (string.Equals(run.CultivationEpoch, MclslWorldEpochSystem.AncientLawEpoch, StringComparison.Ordinal))
        {
            DrawAncientCodexPage(run);
            return;
        }
        switch (_tab)
        {
            case 0:
                DrawPageHeader("玄黄总览", "本世新法修行、背景势力与天地资源的档案总览。");
                GUILayout.BeginHorizontal();
                DrawOverviewPill("当前年份", _snapshot.Year.ToString(), "#CFC7B2", GUILayout.Width(170));
                DrawOverviewPill("世界人口", _snapshot.Population.ToString(), "#9CD7FF", GUILayout.Width(170));
                DrawOverviewPill("修行者", _snapshot.Cultivators.ToString(), "#FFD37A", GUILayout.Width(170));
                DrawOverviewPill("感气", _snapshot.SensingQi.ToString(), "#9CD7FF", GUILayout.Width(145));
                DrawOverviewPill("当前时代", EraDisplay(run), "#A7E08A", GUILayout.Width(190));
                DrawOverviewPill("背景纪元", _snapshot.BackgroundEraName, "#D8C778", GUILayout.Width(190));
                DrawOverviewPill("世界状态", _snapshot.WorldStateName, "#D8C778", GUILayout.Width(190));
                DrawOverviewPill("天地之魄", (run.WorldSouls?.Count ?? 0).ToString(), "#B7A7FF", GUILayout.Width(170));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(8);
                DrawInfoCard("时代规则", "#A7E08A", () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawTag("仙道纪元", "#CFC7B2");
                    DrawTag("传法新法", "#B7A7FF");
                    DrawTag(EraRuleText(run), "#FFD37A");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label("背景纪元：" + _snapshot.BackgroundEraName + "。");
                    if (!string.IsNullOrWhiteSpace(_snapshot.BackgroundEraSummary))
                        GUILayout.Label(_snapshot.BackgroundEraSummary);
                    GUILayout.Label("世界状态：" + _snapshot.WorldStateName + "。");
                    if (!string.IsNullOrWhiteSpace(_snapshot.WorldStateSummary))
                        GUILayout.Label(_snapshot.WorldStateSummary);
                    GUILayout.Label(EraDescription(run));
                });
                DrawInfoCard("七境人数", "#9CD7FF", () =>
                {
                    GUILayout.BeginHorizontal();
                    int column = 0;
                    foreach (string realm in MclslRealmIds.Ordered)
                    {
                        if (_snapshot.RealmCounts.TryGetValue(realm, out int count))
                        {
                            DrawMiniStat(MclslRealmIds.Display(realm), count.ToString(), "#FFD37A", GUILayout.Width(185));
                            column++;
                            if (column % 4 == 0)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                });
                MclslBackgroundFactionState factions = run.BackgroundFactions ?? new MclslBackgroundFactionState();
                DrawInfoCard("万仙盟", "#9CD7FF", () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawTag("影响 " + factions.WanXianAllianceInfluence + "%", "#9CD7FF");
                    DrawTag(factions.AlliancePolicy, "#CFC7B2");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label("秩序压力：" + factions.AllianceOrderPressure);
                    DrawFactionRows(_snapshot.WanXianRecentMissions, _snapshot.WanXianRecentPressure, "#9CD7FF");
                });
                DrawInfoCard("五老会", "#FFD37A", () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawTag("影响 " + factions.FiveEldersInfluence + "%", "#FFD37A");
                    DrawTag(factions.FiveEldersPolicy, "#CFC7B2");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label("暗线渗透：" + factions.FiveEldersSubversion);
                    DrawFactionRows(_snapshot.FiveEldersRecentMissions, _snapshot.FiveEldersRecentPressure, "#FFD37A");
                });
                DrawInfoCard("动态档案", "#B7A7FF", () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawMiniStat("奇物/精髓", (run.GeneratedItems?.Count ?? 0).ToString(), "#FFD37A", GUILayout.Width(170));
                    DrawMiniStat("洞天", (run.WorldCaves?.Count ?? 0).ToString(), "#9CD7FF", GUILayout.Width(150));
                    DrawMiniStat("天地之变", (run.WorldChanges?.Count ?? 0).ToString(), "#B7A7FF", GUILayout.Width(170));
                    DrawMiniStat("宗门遗迹", (run.SectRuins?.Count ?? 0).ToString(), "#A7E08A", GUILayout.Width(170));
                    DrawMiniStat("死亡记录", (run.DeathRecords?.Count ?? 0).ToString(), "#FF8877", GUILayout.Width(170));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label("还真：" + MclslHuanzhenSystem.StatusText());
                    GUILayout.Label("天地之魄：显化 " + _snapshot.WorldSoulManifestedCount + "｜已祭炼 " + _snapshot.WorldSoulHeldCount);
                });
                DrawRuntimeHealthCard(run);
                break;
            case 1:
                DrawRealmRequirements();
                GUILayout.Space(14);
                DrawResourceRules();
                break;
            case 2:
                DrawKingdomDistribution("原生诸国", "尚无国家拥有修士。");
                break;
            case 3:
                DrawCaves(run);
                break;
            case 4:
                DrawWorldChanges(run);
                break;
            case 5:
                DrawWorldSouls(run);
                break;
            case 6:
                DrawInverseTruths(run);
                break;
            case 7:
                DrawRuins(run);
                break;
            case 8:
                DrawRuins(run, true);
                break;
            case 9:
                DrawDaoStruggle(run);
                break;
            case 10:
                DrawDeaths(run);
                break;
            case 11:
                DrawHuanzhen();
                break;
            case 12:
                DrawWorldEvents(run);
                break;
        }
    }

    private void DrawAncientCodexPage(MclslWorldRunState run)
    {
        switch (_tab)
        {
            case 0:
                DrawAncientOverview(run);
                break;
            case 1:
                DrawAncientCultivationPage(run);
                break;
            case 2:
                DrawAncientTeachingPage(run);
                break;
            case 3:
                DrawAncientBreakthroughPage(run);
                break;
            case 4:
                DrawAncientMindPage(run);
                break;
            case 5:
                DrawKingdomDistribution("原生诸国", "尚无凡俗国度拥有仙修。");
                break;
            case 6:
                DrawAncientDisasterPage(run);
                break;
            case 7:
                DrawAncientSecretRealms(run);
                break;
            case 8:
                DrawAncientRuins(run);
                break;
            case 9:
                DrawAncientEventsFromSnapshot("山河观悟", _snapshot.AncientWorldSoulObservationEvents, "暂无高境仙修观悟山河道痕的记录。");
                break;
            default:
                DrawAncientEvents(run);
                break;
        }
    }

    private void DrawAncientOverview(MclslWorldRunState run)
    {
        DrawPageHeader("仙道总览", "仙道纪元，玄黄界以灵根、功法、真元、神魂与大道感悟修行。");
        GUILayout.BeginHorizontal();
        DrawOverviewPill("开始年份", Math.Max(0, run?.EraOriginYear ?? 0).ToString(), "#CFC7B2", GUILayout.Width(170));
        DrawOverviewPill("当前年份", _snapshot.Year.ToString(), "#CFC7B2", GUILayout.Width(170));
        DrawOverviewPill("仙道修士", _snapshot.Cultivators.ToString(), "#FFD37A", GUILayout.Width(170));
        DrawOverviewPill("感气", _snapshot.SensingQi.ToString(), "#9CD7FF", GUILayout.Width(145));
        DrawOverviewPill("背景纪元", _snapshot.BackgroundEraName, "#A7E08A", GUILayout.Width(190));
        DrawOverviewPill("世界状态", _snapshot.WorldStateName, "#D8C778", GUILayout.Width(190));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        DrawInfoCard("时代规则", "#A7E08A", () =>
        {
            GUILayout.BeginHorizontal();
            DrawTag("灵根入道", "#FFD37A");
            DrawTag("功法修身", "#9CD7FF");
            DrawTag("自证大道", "#B7A7FF");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (!string.IsNullOrWhiteSpace(_snapshot.BackgroundEraSummary))
                GUILayout.Label(_snapshot.BackgroundEraSummary);
            if (!string.IsNullOrWhiteSpace(_snapshot.WorldStateSummary))
                GUILayout.Label(_snapshot.WorldStateSummary);
            GUILayout.Label("高境仙修可观山河道痕、天地气机，以印证自身大道。");
        });

        int pioneerStart = MclslNewLawPioneerSystem.PioneerStartYear;
        if (_snapshot.Year >= pioneerStart - 100 && _snapshot.Year < run.AncientLawEndYear)
        {
            bool pioneerActive = MclslNewLawPioneerSystem.IsPioneerEra(_snapshot.Year);
            DrawInfoCard("新法初传", pioneerActive ? "#8FD8D8" : "#CFC7B2", () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(pioneerActive ? "新法已经初传" : "新法初传将近", pioneerActive ? "#8FD8D8" : "#CFC7B2");
                DrawTag("先行修士 " + Math.Max(0, run.NewLawPioneerCount) + "人", "#FFD37A");
                DrawTag("距传法变世 " + Math.Max(0, run.AncientLawEndYear - _snapshot.Year) + "年", "#B7A7FF");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Label(pioneerActive
                    ? "旧仙道仍是天下主流，少数修士已先行试修新法。此时尚未出现仙法不可同修、仙凡瘴、万仙盟与五老会。"
                    : "天地间已出现新法萌芽的征兆；真正的传法变世尚未到来。" );
            });
        }

        DrawInfoCard("七境人数", "#9CD7FF", () =>
        {
            GUILayout.BeginHorizontal();
            int column = 0;
            foreach (string realm in MclslRealmIds.Ordered)
            {
                int count = _snapshot.RealmCounts.TryGetValue(realm, out int value) ? value : 0;
                DrawMiniStat(MclslRealmIds.Display(realm), count.ToString(), "#FFD37A", GUILayout.Width(180));
                column++;
                if (column % 4 == 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        });

        DrawInfoCard("仙道纪事", "#D8C778", () =>
        {
            if (_snapshot.AncientEventsSorted.Count == 0)
            {
                GUILayout.Label("仙道仍在自然流转，暂无入录大事。");
                return;
            }
            for (int i = 0; i < _snapshot.AncientEventsSorted.Count && i < 5; i++)
                DrawEventCard(_snapshot.AncientEventsSorted[i]);
        });
        DrawRuntimeHealthCard(run);
    }

    private static void DrawAncientCultivationPage(MclslWorldRunState run)
    {
        DrawPageHeader("仙道修行", "仙道以内求、自修和大道体悟逐步蜕变。");
        GUILayout.BeginHorizontal();
        DrawOverviewPill("修行体系", "灵根仙道", "#A7E08A", GUILayout.Width(190));
        DrawOverviewPill("入道资格", "灵根", "#FFD37A", GUILayout.Width(170));
        DrawOverviewPill("核心根基", "真元神魂", "#9CD7FF", GUILayout.Width(210));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        DrawAncientRealmCard(
            MclslRealmIds.LianQi,
            "炼气",
            "灵根吐纳，以气炼身",
            "成年有灵根者感天地灵机，吐纳真元，稳固经脉。",
            "推进依赖灵根、心境、灵石资粮、修行年岁与大道初悟。",
            "感气不成只会回落进度，不记大事，不弹公告。");
        DrawAncientRealmCard(
            MclslRealmIds.ZhuJi,
            "筑基",
            "真元归根，自筑道基",
            "炼气圆满后，以自身真元、灵根根基与道悟凝成道基。",
            "生成道基品质、道基稳定与道基特性，道基成于己身，不假外物。",
            "失败可致经脉受损、道基有缺，极少数走火身亡。");
        DrawAncientRealmCard(
            MclslRealmIds.JinDan,
            "金丹",
            "丹成于身，本命自凝",
            "道基圆融后，真元、神魂与本命道意凝成本命金丹。",
            "金丹围绕角色自身道路成长，丹意随功法与道悟渐深。",
            "失败会损伤纯度、心境或根基，低概率重伤。");
        DrawAncientRealmCard(
            MclslRealmIds.YuanYing,
            "元婴",
            "丹破婴生，神魂成形",
            "金丹温养至极，神魂壮大，丹破而婴生。",
            "元婴记录神魂强度、肉身契合与元婴稳定，根基仍归自身。",
            "失败多为神魂受损、金丹开裂或道途受阻。");
        DrawAncientRealmCard(
            MclslRealmIds.HuaShen,
            "化神",
            "神融大道，化出神意",
            "元婴、神魂与自身大道长期相融，渐成一身神意。",
            "可借地火、天象、灾劫和观悟增长感悟，神意仍由己身大道凝成。",
            "失败会动摇神魂与大道契合，重者闭关多年。");
        DrawAncientRealmCard(
            MclslRealmIds.HeDao,
            "合道",
            "大道归一，合于天地",
            "自身大道得天地承认，方可与玄黄大道相合。",
            "化神真元圆满后，以自身大道与天地气机相合。",
            "合道闭关动辄数十年至数百年，失败会折损道途。");
        DrawAncientRealmCard(
            MclslRealmIds.ChangSheng,
            "长生",
            "仙道尽头，未有定式",
            "本纪元未开放稳定长生链路，长生不按普通四段推进。",
            "若有超脱者，也应由自身大道圆满与特殊世界事件推动。",
            "此境不以常规真元关隘表示。");
    }

    private static void DrawAncientRealmCard(string realmId, string title, string motto, string core, string progress, string failure)
    {
        DrawInfoCard(title, "#9CD7FF", () =>
        {
            GUILayout.BeginHorizontal();
            DrawTag(motto, "#FFD37A");
            DrawTag("灵根仙道", "#A7E08A");
            int essence = MclslRealmProgress.MinimumForRealm(realmId);
            DrawTag(essence > 0 ? "真元 " + essence : "真元 -", essence > 0 ? "#9CD7FF" : "#6F7B86");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("根基：" + core);
            GUILayout.Label("推进：" + progress);
            GUILayout.Label("劫数：" + failure);
        });
    }

    private void DrawAncientTeachingPage(MclslWorldRunState run)
    {
        DrawPageHeader("传承道法", "山河问道，法脉自人间流转。");
        DrawAncientTeachingRealmFilterBar();
        List<MclslTechniqueLineageRecord> lineages = _snapshot.TechniqueLineagesSorted;
        int shown = 0;
        if (lineages != null)
        {
            for (int i = 0; i < lineages.Count && shown < 120; i++)
            {
                MclslTechniqueLineageRecord lineage = lineages[i];
                if (lineage == null || lineage.SystemId != MclslCultivationSystemIds.AncientLaw) continue;
                if (!AncientTeachingMaxRealmMatches(lineage)) continue;
                shown++;
                DrawInfoCard("《" + Blank(lineage.Name) + "》", LineageColor(lineage.LifecycleState), () =>
                {
                    int strength = AncientLineageStrength(lineage);
                    string laws = ReplaceTags(lineage.LawTags);
                    string lineageTitle = AncientLineageTitle(lineage);
                    bool recovered = IsRecoveredAncientLineage(lineage);

                    GUILayout.BeginHorizontal();
                    DrawTag(AncientLineageStrengthLabel(lineage), "#FFD37A");
                    DrawTag(MclslRealmIds.Display(lineage.MaxRealm) + "法门", "#9CD7FF");
                    DrawTag(AncientLineageState(lineage), LineageColor(lineage.LifecycleState));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    DrawMiniStat("功法完整", strength + "%", "#D8C778", GUILayout.Width(140));
                    DrawMiniStat("当前流传", lineage.CurrentPractitioners + "人", "#A7E08A", GUILayout.Width(140));
                    DrawMiniStat("上限境界", MclslRealmIds.Display(AncientLineageDisplayMaxRealm(lineage)), "#9CD7FF", GUILayout.Width(130));
                    DrawMiniStat("创始人", BlankFounder(lineage.FounderName), "#FFD37A", GUILayout.Width(220));
                    if (recovered)
                    {
                        DrawMiniStat("复现形态", AncientLineageRecoveredForm(lineage), "#CFC7B2", GUILayout.Width(150));
                        DrawMiniStat("残卷成色", AncientLineageArchiveGrade(lineage), "#FFD37A", GUILayout.Width(150));
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if (recovered)
                    {
                        GUILayout.Label("<color=#CFC7B2>残卷来历：</color>" + AncientLineageRecoveredSourceText(lineage, lineageTitle));
                        GUILayout.Label("<color=#CFC7B2>复现脉络：</color>" + LineageChainText(lineage));
                        GUILayout.Label("<color=#CFC7B2>功法命数：</color>" + AncientLineageMandateText(lineage, strength));
                    }
                    else
                    {
                        GUILayout.Label("<color=#CFC7B2>传承来历：</color>" + AncientLineageEraSourceText(lineage, lineageTitle));
                        GUILayout.Label("<color=#CFC7B2>流传脉络：</color>" + LineageChainText(lineage));
                    }
                    GUILayout.Label("<color=#CFC7B2>当世流传：</color>" + AncientLineageCurrentText(lineage, lineageTitle));
                    GUILayout.Label("<color=#CFC7B2>修行倾向：</color>" + AncientLineageTendencyText(lineage, laws));
                    GUILayout.Label("<color=#CFC7B2>词条：</color>" + AncientLineageTermsText(lineage, laws));
                });
            }
        }
        if (shown == 0) DrawEmptyCard("暂无传承", "当前筛选下暂无功法传承入录。");
    }

    private void DrawAncientTeachingRealmFilterBar()
    {
        GUILayout.BeginHorizontal();
        AncientTeachingRealmButton("全部", MclslEventCatalog.All, CountAncientTeachingByMaxRealm(MclslEventCatalog.All));
        foreach (string realm in MclslRealmIds.Ordered)
            AncientTeachingRealmButton(MclslRealmIds.Display(realm), realm, CountAncientTeachingByMaxRealm(realm));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(6);
    }

    private void AncientTeachingRealmButton(string label, string realmId, int count)
    {
        GUI.backgroundColor = string.Equals(_ancientTeachingMaxRealmFilter, realmId, StringComparison.Ordinal) ? new Color(0.35f, 0.35f, 0.35f) : Color.gray;
        if (GUILayout.Button(label + " " + count, GUILayout.Width(92f), GUILayout.Height(30f)))
        {
            _ancientTeachingMaxRealmFilter = realmId;
            _scroll = Vector2.zero;
        }
        GUI.backgroundColor = Color.white;
    }

    private int CountAncientTeachingByMaxRealm(string realmId)
    {
        List<MclslTechniqueLineageRecord> lineages = _snapshot.TechniqueLineagesSorted;
        if (lineages == null) return 0;
        int count = 0;
        bool all = string.Equals(realmId, MclslEventCatalog.All, StringComparison.Ordinal);
        for (int i = 0; i < lineages.Count; i++)
        {
            MclslTechniqueLineageRecord lineage = lineages[i];
            if (lineage == null || lineage.SystemId != MclslCultivationSystemIds.AncientLaw) continue;
            if (all || string.Equals(NormalizeRealm(lineage.MaxRealm), realmId, StringComparison.Ordinal)) count++;
        }
        return count;
    }

    private bool AncientTeachingMaxRealmMatches(MclslTechniqueLineageRecord lineage)
    {
        return string.Equals(_ancientTeachingMaxRealmFilter, MclslEventCatalog.All, StringComparison.Ordinal)
            || string.Equals(NormalizeRealm(lineage.MaxRealm), _ancientTeachingMaxRealmFilter, StringComparison.Ordinal);
    }

    private static string NormalizeRealm(string realmId)
    {
        return MclslRealmIds.Index(realmId) >= 0 ? realmId : MclslRealmIds.LianQi;
    }

    private void DrawAncientBreakthroughPage(MclslWorldRunState run)
    {
        DrawAncientRealmFilterBar(_snapshot.AncientBreakthroughEvents, ref _ancientBreakthroughRealmFilter);
        DrawAncientEventsFromSnapshot("仙道破境", _snapshot.AncientBreakthroughEvents, "暂无仙道破境或破境失败记录。", _ancientBreakthroughRealmFilter);
    }

    private void DrawAncientMindPage(MclslWorldRunState run)
    {
        DrawAncientRealmFilterBar(_snapshot.AncientMindEvents, ref _ancientMindRealmFilter);
        DrawAncientEventsFromSnapshot("心境劫数", _snapshot.AncientMindEvents, "暂无闭关、顿悟或心魔记录。", _ancientMindRealmFilter);
    }

    private void DrawAncientDisasterPage(MclslWorldRunState run)
    {
        DrawAncientEventsFromSnapshot("灵机灾变", _snapshot.AncientDisasterEvents, "暂无灵气汇聚、地火、星石或地脉衰退记录。");
    }

    private void DrawAncientRuins(MclslWorldRunState run)
    {
        DrawRuins(run, false);
    }

    private void DrawAncientSecretRealms(MclslWorldRunState run)
    {
        DrawRuins(run, true);
    }

    private void DrawAncientEvents(MclslWorldRunState run)
    {
        DrawPageHeader("仙道纪事", "千年仙道，风云入录。");
        DrawAncientEventFilterBar();
        IReadOnlyList<MclslRunEventRecord> events = AncientEventsForFilter(_ancientEventFilter);
        int shown = 0;
        if (events != null)
        {
            for (int i = 0; i < events.Count && shown < 180; i++)
            {
                MclslRunEventRecord record = events[i];
                if (record == null) continue;
                DrawEventCard(record);
                shown++;
            }
        }
        if (shown == 0) DrawEmptyCard("暂无纪事", "当前筛选下暂无入录大事。");
    }

    private void DrawAncientEventFilterBar()
    {
        GUILayout.BeginHorizontal();
        AncientEventFilterButton("全部", MclslEventCatalog.All, _snapshot.AncientEventsSorted?.Count ?? 0);
        AncientEventFilterButton("授法", "teaching", _snapshot.AncientTeachingEvents?.Count ?? 0);
        AncientEventFilterButton("破境", "breakthrough", _snapshot.AncientBreakthroughEvents?.Count ?? 0);
        AncientEventFilterButton("心境", "mind", _snapshot.AncientMindEvents?.Count ?? 0);
        AncientEventFilterButton("灾变", "disaster", _snapshot.AncientDisasterEvents?.Count ?? 0);
        AncientEventFilterButton("秘境", "secret", _snapshot.AncientSecretRealmEvents?.Count ?? 0);
        AncientEventFilterButton("山河", "observation", _snapshot.AncientWorldSoulObservationEvents?.Count ?? 0);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(6);
    }

    private void AncientEventFilterButton(string label, string key, int count)
    {
        GUI.backgroundColor = string.Equals(_ancientEventFilter, key, StringComparison.Ordinal) ? new Color(0.35f, 0.35f, 0.35f) : Color.gray;
        if (GUILayout.Button(label + " " + count, GUILayout.Width(92f), GUILayout.Height(30f)))
        {
            _ancientEventFilter = key;
            _scroll = Vector2.zero;
        }
        GUI.backgroundColor = Color.white;
    }

    private IReadOnlyList<MclslRunEventRecord> AncientEventsForFilter(string key)
    {
        return key switch
        {
            "teaching" => _snapshot.AncientTeachingEvents,
            "breakthrough" => _snapshot.AncientBreakthroughEvents,
            "mind" => _snapshot.AncientMindEvents,
            "disaster" => _snapshot.AncientDisasterEvents,
            "secret" => _snapshot.AncientSecretRealmEvents,
            "observation" => _snapshot.AncientWorldSoulObservationEvents,
            _ => _snapshot.AncientEventsSorted
        };
    }

    private void DrawAncientEventsFromSnapshot(string title, IReadOnlyList<MclslRunEventRecord> events, string empty)
    {
        DrawAncientEventsFromSnapshot(title, events, empty, MclslEventCatalog.All);
    }

    private void DrawAncientEventsFromSnapshot(string title, IReadOnlyList<MclslRunEventRecord> events, string empty, string realmFilter)
    {
        DrawPageHeader(title, "山河旧事，散入玄黄。");
        int shown = 0;
        if (events != null)
        {
            for (int i = 0; i < events.Count && shown < 160; i++)
            {
                MclslRunEventRecord record = events[i];
                if (record == null) continue;
                if (!AncientEventRealmMatches(record, realmFilter)) continue;
                DrawEventCard(record);
                shown++;
            }
        }
        if (shown == 0) DrawEmptyCard("暂无记录", empty);
    }

    private void DrawAncientRealmFilterBar(IReadOnlyList<MclslRunEventRecord> events, ref string filter)
    {
        GUILayout.BeginHorizontal();
        AncientRealmFilterButton("全部", MclslEventCatalog.All, CountAncientEventsByRealm(events, MclslEventCatalog.All), ref filter);
        AncientRealmFilterButton("金丹", MclslRealmIds.JinDan, CountAncientEventsByRealm(events, MclslRealmIds.JinDan), ref filter);
        AncientRealmFilterButton("元婴", MclslRealmIds.YuanYing, CountAncientEventsByRealm(events, MclslRealmIds.YuanYing), ref filter);
        AncientRealmFilterButton("化神", MclslRealmIds.HuaShen, CountAncientEventsByRealm(events, MclslRealmIds.HuaShen), ref filter);
        AncientRealmFilterButton("合道", MclslRealmIds.HeDao, CountAncientEventsByRealm(events, MclslRealmIds.HeDao), ref filter);
        AncientRealmFilterButton("长生", MclslRealmIds.ChangSheng, CountAncientEventsByRealm(events, MclslRealmIds.ChangSheng), ref filter);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(6);
    }

    private void AncientRealmFilterButton(string label, string realmId, int count, ref string filter)
    {
        GUI.backgroundColor = string.Equals(filter, realmId, StringComparison.Ordinal) ? new Color(0.35f, 0.35f, 0.35f) : Color.gray;
        if (GUILayout.Button(label + " " + count, GUILayout.Width(92f), GUILayout.Height(30f)))
        {
            filter = realmId;
            _scroll = Vector2.zero;
        }
        GUI.backgroundColor = Color.white;
    }

    private static int CountAncientEventsByRealm(IReadOnlyList<MclslRunEventRecord> events, string realmId)
    {
        if (events == null) return 0;
        int count = 0;
        for (int i = 0; i < events.Count; i++)
            if (AncientEventRealmMatches(events[i], realmId)) count++;
        return count;
    }

    private static bool AncientEventRealmMatches(MclslRunEventRecord record, string realmId)
    {
        if (record == null) return false;
        if (string.Equals(realmId, MclslEventCatalog.All, StringComparison.Ordinal)) return true;
        string realmName = MclslRealmIds.Display(realmId);
        string title = record.Title ?? string.Empty;
        string body = record.Body ?? string.Empty;
        return title.Contains(realmName) || body.Contains(realmName);
    }

    private static void DrawEventCard(MclslRunEventRecord e)
    {
        MclslEventCategoryDefinition category = MclslEventCatalog.Category(EventCategory(e));
        DrawInfoCard(e.Year + "年｜" + e.Title, category.Color, () =>
        {
            GUILayout.BeginHorizontal();
            DrawTag(category.Name, category.Color);
            GUILayout.FlexibleSpace();
            if (MclslEventLocator.CanLocate(e) && GUILayout.Button("定位", GUILayout.Width(62f)))
                MclslEventLocator.Locate(e);
            GUILayout.EndHorizontal();
            GUILayout.Label(PlayerFacingEventBody(e.Body));
        });
    }

    private static void DrawEmptyCard(string title, string body)
    {
        DrawInfoCard(title, "#CFC7B2", () => GUILayout.Label(body));
    }

    private void DrawWorldSouls(MclslWorldRunState run)
    {
        DrawPageHeader("天地之魄", "魄位显隐，天职流转。");
        GUILayout.BeginHorizontal();
        DrawOverviewPill("魄位", _snapshot.WorldSoulsSorted.Count.ToString(), "#B7A7FF", GUILayout.Width(150));
        DrawOverviewPill("显化", _snapshot.WorldSoulManifestedCount.ToString(), "#FFD37A", GUILayout.Width(150));
        DrawOverviewPill("已祭炼", _snapshot.WorldSoulHeldCount.ToString(), "#A7E08A", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        foreach (MclslWorldSoulRecord soul in _snapshot.WorldSoulsSorted)
        {
            DrawInfoCard(soul.Name, QualityColor(soul.Quality), () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(Quality(soul.Quality), QualityColor(soul.Quality));
                DrawTag(soul.State, soul.State == "显化" ? "#FFD37A" : soul.State == "已祭炼" ? "#A7E08A" : "#B8B8B8");
                DrawTag(ReplaceTags(soul.LawTags), "#9CD7FF");
                GUILayout.FlexibleSpace();
                if (MclslEventLocator.CanLocate(soul) && GUILayout.Button("定位", GUILayout.Width(64))) MclslEventLocator.Locate(soul);
                GUILayout.EndHorizontal();
                GUILayout.Label("天职：" + soul.HeavenlyDuty);
                GUILayout.Label("承接者：" + (soul.HolderActorId > 0 ? Blank(soul.HolderActorName) + "（" + soul.HolderYear + "年）" : "无"));
                GUILayout.Label("履职 " + soul.DutyProgress + "%｜反噬 " + soul.DutyBacklash + "%｜显化 " + soul.ManifestCount + " 次");
                if (soul.NextManifestYear > 0 && soul.HolderActorId <= 0) GUILayout.Label("下次最早显化：" + soul.NextManifestYear + "年");
            });
        }
        if (_snapshot.WorldSoulsSorted.Count == 0) GUILayout.Label("本世尚无预设天地之魄。\n");
    }

    private void DrawInverseTruths(MclslWorldRunState run)
    {
        DrawPageHeader("天地之理", "天地有常，亦有逆流。");
        List<MclslInverseTruthRecord> visiblePlayerTruths = _snapshot.VisiblePlayerTruths;
        List<MclslInverseTruthRecord> canonTruths = _snapshot.CanonTruths;
        GUILayout.BeginHorizontal();
        DrawOverviewPill("本世显现", visiblePlayerTruths.Count.ToString(), "#B7A7FF", GUILayout.Width(170));
        DrawOverviewPill("已证长生", _snapshot.ReversedPlayerTruthCount + "/" + MclslRealmSeatSystem.LongevityLimitCurrent(), "#D8C778", GUILayout.Width(170));
        DrawOverviewPill("旧世存理", canonTruths.Count.ToString(), "#A7E08A", GUILayout.Width(170));
        DrawOverviewPill("轮回转世", _snapshot.ReincarnationAppliedCount.ToString(), "#7CCFD0", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        DrawInfoCard("本世可证逆理", "#B7A7FF", () =>
        {
            foreach (MclslInverseTruthRecord truth in visiblePlayerTruths)
            {
                GUILayout.BeginHorizontal();
                DrawTag(truth.Name, truth.Reversed ? "#A7E08A" : "#B7A7FF");
                DrawTag(truth.Reversed ? "已证" : "逆理中", truth.Reversed ? "#A7E08A" : "#B7A7FF");
                DrawTag("进度 " + truth.Progress + "%", "#FFD37A");
                if (truth.ChallengerActorId > 0)
                    DrawTag((truth.Reversed ? "证道者 " : "承理者 ") + TruthActorName(truth), "#9CD7FF");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Label(truth.RuleDescription);
                GUILayout.Space(4);
            }
            if (visiblePlayerTruths.Count == 0) GUILayout.Label("本世尚无人触及新的天地之理。");
        });

        DrawInfoCard("旧世存理", "#A7E08A", () =>
        {
            foreach (MclslInverseTruthRecord truth in canonTruths)
                DrawInfoCard(truth.Name, "#A7E08A", () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawTag("背景规则", "#CFC7B2");
                    DrawTag(truth.Reversed ? "已存世" : "沉入旧纪", "#A7E08A");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label(truth.RuleDescription);
                });
            if (canonTruths.Count == 0) GUILayout.Label("旧纪诸理尚未入录。");
        });

        DrawReincarnationArchive();
    }

    private void DrawReincarnationArchive()
    {
        DrawInfoCard("轮回转世", "#7CCFD0", () =>
        {
            GUILayout.BeginHorizontal();
            DrawTag("已转 " + _snapshot.ReincarnationAppliedCount, "#7CCFD0");
            DrawTag("待转 " + _snapshot.ReincarnationPendingCount, "#CFC7B2");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (_snapshot.ReincarnationRecordsSorted.Count == 0)
            {
                GUILayout.Label("暂无转世记录。");
                return;
            }

            int shown = 0;
            foreach (MclslActorReincarnationRecord record in _snapshot.ReincarnationRecordsSorted)
            {
                if (shown++ >= 12) break;
                string statusColor = string.Equals(record.Status, "已转", StringComparison.Ordinal) ? "#7CCFD0" : "#CFC7B2";
                string yearText = record.AppliedYear > 0 ? record.AppliedYear + "年" : record.DeathYear + "年";
                string title = string.Equals(record.Status, "已转", StringComparison.Ordinal)
                    ? Blank(record.TargetActorName) + "承前世"
                    : Blank(record.SourceActorName) + "识未尽";
                DrawInfoCard(yearText + "｜" + title, statusColor, () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawTag(record.Status, statusColor);
                    DrawTag(Blank(record.SourceRealmName), RealmTagColor(record.SourceRealmId));
                    if (record.RootPurityFloor > 0) DrawTag("灵根底限 " + record.RootPurityFloor, "#FFD37A");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label("前世：" + Blank(record.SourceActorName)
                        + (string.IsNullOrWhiteSpace(record.SourceTechniqueName) ? string.Empty : "，《" + record.SourceTechniqueName + "》"));
                    if (record.TargetActorId > 0)
                        GUILayout.Label("今生：" + Blank(record.TargetActorName));
                });
            }
        });
    }

    private static string TruthActorName(MclslInverseTruthRecord truth)
    {
        if (truth == null || truth.ChallengerActorId <= 0) return "无主";
        try
        {
            if (MclslCultivatorCandidateIndex.Resolve(truth.ChallengerActorId, out Actor actor))
                return MclslActorAccessor.DisplayName(actor);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslCodexWindow-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslCodexWindow.cs #1: " + mclslEmptyCatchEx.Message); }
        return "行踪不明";
    }

    private static void DrawRealmRequirements()
    {
        DrawPageHeader("七境纲领与突破要求", "新法七境不是单纯数值提升，而是逐层从天地夺取更高层级的资源。灵根品阶、属性、数量和纯度影响修行效率、悟法池与承受稳定，但不替代天地资源。");
        GUILayout.BeginHorizontal();
        DrawOverviewPill("境界", MclslRealmIds.Ordered.Length.ToString(), "#9CD7FF", GUILayout.Width(170));
        DrawOverviewPill("突破路径", "逐境夺取", "#FFD37A", GUILayout.Width(190));
        DrawOverviewPill("合道路径", "祭魄即成", "#B7A7FF", GUILayout.Width(210));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        foreach (MclslRealmRequirementDefinition requirement in MclslRealmRequirementCatalog.Requirements)
        {
            DrawInfoCard(MclslRealmIds.Display(requirement.RealmId), "#9CD7FF", () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(requirement.Motto, "#FFD37A");
                DrawTag(requirement.CoreObject, "#CFC7B2");
                int essence = MclslRealmProgress.MinimumForRealm(requirement.RealmId);
                DrawTag(essence > 0 ? "真元 " + essence : "真元 -", essence > 0 ? "#9CD7FF" : "#6F7B86");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Label("进入条件：" + requirement.EntryRequirement);
                GUILayout.Label("突破要求：" + requirement.BreakthroughRequirement);
                GUILayout.Label("世界交互：" + requirement.WorldInteraction);
                GUILayout.Label("失败风险：" + requirement.FailureRisk);
                if (requirement.RealmId == MclslRealmIds.LianQi)
                GUILayout.Label("补救入门：炼心法门圆满、洗瘴池洗去凡浊，都可让凡人踏入新法。");
                if (requirement.RealmId == MclslRealmIds.ZhuJi)
        GUILayout.Label("奇物来源：年度筑基机缘、宗门遗迹线索、功法与自身法则池映照；奇物品质不由灵根品阶直接决定。");
                if (requirement.RealmId == MclslRealmIds.YuanYing)
                    GUILayout.Label("洞天争夺：金丹法则、纯度协调、洞天残图和心境共同影响争夺强度与炼化成功。");
                if (requirement.RealmId == MclslRealmIds.HuaShen)
                    GUILayout.Label("抽髓承受：元婴根基与心境影响抽取天地之髓的成功率和反噬风险。");
            });
        }
    }

    private void DrawFoundationWonders(MclslWorldRunState run)
    {
        DrawPageHeader("筑基奇物谱", "天地奇物分人之奇、地之奇、天之奇。人之奇分上中下品；地之奇重完整度与规则规模；天之奇极罕见，可支撑更高大道但极难悟解。");
        List<MclslGeneratedItemRecord> wonders = _snapshot.FoundationWondersSorted;
        GUILayout.BeginHorizontal();
        DrawOverviewPill("本世奇物", wonders.Count.ToString(), "#FFD37A", GUILayout.Width(170));
        DrawOverviewPill("天之奇", _snapshot.FoundationHeavenCount.ToString(), "#FF5A5A", GUILayout.Width(150));
        DrawOverviewPill("地之奇", _snapshot.FoundationEarthCount.ToString(), "#9CD7FF", GUILayout.Width(150));
        DrawOverviewPill("已炼化", _snapshot.FoundationRefinedCount.ToString(), "#9CD7FF", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        if (wonders.Count == 0) GUILayout.Label("本世尚未生成筑基奇物。");
        foreach (MclslGeneratedItemRecord item in wonders)
        {
            string rank = MclslGeneratedObjectFactory.FoundationRankText(item);
            string color = MclslGeneratedObjectFactory.FoundationRankColor(item.Category);
            DrawItemCard(item.Name, rank, ReplaceTags(item.LawTags), DisplayHolder(item), item.Origin, RootText(item), item.Description, color);
        }
    }

    private void DrawCaves(MclslWorldRunState run)
    {
        DrawPageHeader("元婴洞天与天地之精", "金丹圆满者按法则适配、金丹纯度协调、洞天残图、心境共同争夺；每次炼化都会消耗天地之精并损耗洞天完整度。");
        List<MclslWorldCaveRecord> caves = _snapshot.CavesSorted;
        List<MclslGeneratedItemRecord> essences = _snapshot.HeavenEarthEssencesSorted;
        GUILayout.BeginHorizontal();
        DrawOverviewPill("洞天总数", caves.Count.ToString(), "#9CD7FF", GUILayout.Width(170));
        DrawOverviewPill("活跃", _snapshot.ActiveCaveCount.ToString(), "#A7E08A", GUILayout.Width(150));
        DrawOverviewPill("已炼精", essences.Count.ToString(), "#FFD37A", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        if (caves.Count == 0) GUILayout.Label("本世尚无洞天显化。");
        foreach (MclslWorldCaveRecord cave in caves)
        {
            DrawInfoCard(cave.Name, QualityColor(cave.Quality), () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(Quality(cave.Quality), QualityColor(cave.Quality));
                DrawTag(cave.State, cave.State == "活跃" ? "#A7E08A" : cave.State == "衰退" ? "#FFD37A" : "#B8B8B8");
                DrawTag(ReplaceTags(cave.LawTags), "#9CD7FF");
                GUILayout.FlexibleSpace();
                if (MclslEventLocator.CanLocate(cave) && GUILayout.Button("定位", GUILayout.Width(64))) MclslEventLocator.Locate(cave);
                GUILayout.EndHorizontal();
                GUILayout.Label("位置：" + cave.NativeKingdomName + "·" + cave.LocationName);
                GUILayout.Label("余精 " + cave.RemainingEssence + "/" + cave.EssenceCapacity + "｜完整度 " + cave.Integrity + "%｜已炼化 " + cave.RefinedCount + " 次");
                GUILayout.Label(cave.Description);
            });
        }
        DrawPageHeader("已炼化天地之精", "");
        if (essences.Count == 0) GUILayout.Label("本世尚无人炼化天地之精。");
        foreach (MclslGeneratedItemRecord item in essences)
            DrawItemCard(item.Name, Quality(item.Quality), ReplaceTags(item.LawTags), DisplayHolder(item), item.CreatedYear + "年", "", item.Description, QualityColor(item.Quality));
    }

    private void DrawWorldChanges(MclslWorldRunState run)
    {
        DrawPageHeader("天地之变与化神抽髓", "元婴圆满者按法则适配参与抽髓；失败可能受创或当场陨落，成功则炼得天地之髓并成就化神。");
        List<MclslWorldChangeRecord> changes = _snapshot.WorldChangesSorted;
        List<MclslGeneratedItemRecord> marrows = _snapshot.WorldChangeMarrowsSorted;
        GUILayout.BeginHorizontal();
        DrawOverviewPill("天地之变", changes.Count.ToString(), "#B7A7FF", GUILayout.Width(170));
        DrawOverviewPill("活跃", _snapshot.ActiveWorldChangeCount.ToString(), "#A7E08A", GUILayout.Width(150));
        DrawOverviewPill("已抽髓", marrows.Count.ToString(), "#FFD37A", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        if (changes.Count == 0) GUILayout.Label("当前尚无天地之变。");
        foreach (MclslWorldChangeRecord change in changes)
        {
            string end = change.EndYear > 0 ? "｜平息于" + change.EndYear + "年" : string.Empty;
            DrawInfoCard(change.Name, QualityColor(change.Quality), () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(Quality(change.Quality), QualityColor(change.Quality));
                DrawTag(change.State, change.State == "活跃" ? "#A7E08A" : change.State == "衰减" ? "#FFD37A" : "#B8B8B8");
                DrawTag(ReplaceTags(change.LawTags), "#9CD7FF");
                GUILayout.FlexibleSpace();
                if (MclslEventLocator.CanLocate(change) && GUILayout.Button("定位", GUILayout.Width(64))) MclslEventLocator.Locate(change);
                GUILayout.EndHorizontal();
                GUILayout.Label("位置：" + change.NativeKingdomName + "·" + change.LocationName + "｜始于" + change.StartYear + "年" + end);
                GUILayout.Label("余髓 " + change.RemainingMarrow + "/" + change.MarrowCapacity + "｜强度 " + change.Intensity + "%｜已抽取 " + change.ExtractedCount + " 次");
                GUILayout.Label("缘起：<color=#B9B0A0>" + change.Origin + "</color>");
                GUILayout.Label(change.Description);
            });
        }
        DrawPageHeader("已炼化天地之髓", "");
        if (marrows.Count == 0) GUILayout.Label("本世尚无人抽得天地之髓。");
        foreach (MclslGeneratedItemRecord item in marrows)
            DrawItemCard(item.Name, Quality(item.Quality), ReplaceTags(item.LawTags), DisplayHolder(item), item.CreatedYear + "年", RootText(item), item.Description, QualityColor(item.Quality));
    }

    private void DrawDeaths(MclslWorldRunState run)
    {
        DrawPageHeader("修士生死簿", "生死簿记录修士道途终点，也为天地之变、还真与逆理留下可追溯的世界事实。");
        List<MclslDeathRecord> deaths = _snapshot.DeathsByYear;
        List<MclslDeathRecord> filtered = string.Equals(_deathRealmFilter, MclslEventCatalog.All, StringComparison.Ordinal)
            ? deaths
            : (_snapshot.DeathsByRealm.TryGetValue(_deathRealmFilter, out List<MclslDeathRecord> realmDeaths) ? realmDeaths : new List<MclslDeathRecord>());
        GUILayout.BeginHorizontal();
        DrawOverviewPill("死亡记录", deaths.Count.ToString(), "#FF8877", GUILayout.Width(170));
        DrawOverviewPill("当前筛选", string.Equals(_deathRealmFilter, MclslEventCatalog.All, StringComparison.Ordinal) ? "全部" : MclslRealmIds.Display(_deathRealmFilter), "#9CD7FF", GUILayout.Width(190));
        DrawOverviewPill("近期显示", Math.Min(300, filtered.Count).ToString(), "#CFC7B2", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        DeathRealmButton("全部", MclslEventCatalog.All);
        foreach (string realm in MclslRealmIds.Ordered)
            DeathRealmButton(MclslRealmIds.Display(realm), realm);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        if (filtered.Count == 0) GUILayout.Label("当前筛选下尚无修士死亡记录。");
        int shown = 0;
        foreach (MclslDeathRecord death in filtered)
        {
            if (shown++ >= 300) break;
            DrawDeathCard(death);
        }
    }

    private static void DrawDeathCard(MclslDeathRecord death)
    {
        if (death == null) return;
        string place = string.IsNullOrWhiteSpace(death.KingdomName) && string.IsNullOrWhiteSpace(death.CityName)
            ? "去处不详"
            : (string.IsNullOrWhiteSpace(death.KingdomName) ? "无国" : death.KingdomName) + "·" + (string.IsNullOrWhiteSpace(death.CityName) ? "无城" : death.CityName);
        DrawInfoCard(death.Year + "年｜" + (string.IsNullOrWhiteSpace(death.Title) ? death.ActorName : death.Title), "#FF8877", () =>
        {
            GUILayout.BeginHorizontal();
            DrawTag(Blank(death.RealmName), "#9CD7FF");
            DrawTag("修道 " + death.Age + "载", "#CFC7B2");
            DrawTag(Blank(death.CauseText), "#FF8877");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUI.skin.box);
            DrawMiniStat("地点", place, "#CFC7B2", GUILayout.Width(260));
            DrawMiniStat("主修", Blank(death.TechniqueName), "#FFD37A", GUILayout.Width(260));
            if (!string.IsNullOrWhiteSpace(death.KillerName))
                DrawMiniStat("凶手", death.KillerName, "#FF8877", GUILayout.Width(220));
            GUILayout.FlexibleSpace();
            if (MclslEventLocator.CanLocate(death) && GUILayout.Button("定位", GUILayout.Width(62f)))
                MclslEventLocator.Locate(death);
            GUILayout.EndHorizontal();

            DrawDeathLineage(death);
            GUILayout.Space(4);
            GUILayout.Label("<b><color=#FFB0A8>天诛悼文</color></b>");
            GUILayout.Label(PlayerFacingEventBody(death.Announcement));
        });
    }

    private static void DrawDeathLineage(MclslDeathRecord death)
    {
        bool any = HasValue(death.FoundationWonderName) || HasValue(death.GoldenCoreLaws)
            || HasValue(death.NascentCaveName) || HasValue(death.NascentEssenceName)
            || HasValue(death.DivineChangeName) || HasValue(death.DivineMarrowName)
            || HasValue(death.WorldSoulName) || HasValue(death.InverseTruthName);
        if (!any) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("<b><color=#FFD37A>道途遗痕</color></b>");
        GUILayout.BeginHorizontal();
        int column = 0;
        DrawDeathField("筑基", death.FoundationWonderName, "#FFD37A", ref column);
        DrawDeathField("金丹", ReplaceTags(death.GoldenCoreLaws), "#D8C778", ref column);
        DrawDeathField("洞天", death.NascentCaveName, "#9CD7FF", ref column);
        DrawDeathField("天地之精", death.NascentEssenceName, "#9CD7FF", ref column);
        DrawDeathField("天地之变", death.DivineChangeName, "#B7A7FF", ref column);
        DrawDeathField("天地之髓", death.DivineMarrowName, "#B7A7FF", ref column);
        DrawDeathField("天地之魄", death.WorldSoulName, "#7CCFD0", ref column);
        DrawDeathField("逆理", death.InverseTruthName, "#A7E08A", ref column);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private static void DrawDeathField(string label, string value, string color, ref int column)
    {
        if (!HasValue(value)) return;
        DrawMiniStat(label, value, color, GUILayout.Width(220));
        column++;
        if (column % 4 == 0)
        {
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
        }
    }

    private static bool HasValue(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value != "无";
    }

    private void DeathRealmButton(string label, string realmId)
    {
        GUI.backgroundColor = string.Equals(_deathRealmFilter, realmId, StringComparison.Ordinal) ? new Color(0.35f, 0.35f, 0.35f) : Color.gray;
        if (GUILayout.Button(label, GUILayout.Width(88f), GUILayout.Height(30f)))
        {
            _deathRealmFilter = realmId;
            _scroll = Vector2.zero;
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawHuanzhen()
    {
        DrawPageHeader("还真轮回", "此世还真未降临。");
        MclslHuanzhenExternalState state = MclslHuanzhenSystem.Current;
        GUILayout.BeginHorizontal();
        DrawOverviewPill("功能状态", MclslHuanzhenSystem.StatusText(), "#B8B8B8", GUILayout.Width(210));
        DrawOverviewPill("持有者", Blank(state.HostName), "#FFD37A", GUILayout.Width(210));
        DrawOverviewPill("避环层数", state.ConsecutiveLoopDeaths.ToString(), "#B7A7FF", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        DrawInfoCard("当前绑定", "#CFC7B2", () =>
        {
            GUILayout.Label("最近锚点：" + (state.LastAnchorYear < 0 ? "无" : state.LastAnchorYear + "年"));
            GUILayout.Label("最近回溯：" + (state.LastRestoreYear < 0 ? "无" : state.LastRestoreYear + "年"));
        });

        DrawPageHeader("滚动锚点", "");
        if (_snapshot.HuanzhenAnchorsSorted.Count == 0) GUILayout.Label("当前没有可用还真锚点。");
        else
        {
            foreach (MclslHuanzhenAnchorRecord anchor in _snapshot.HuanzhenAnchorsSorted)
                DrawInfoCard(anchor.Year + "年｜" + Blank(anchor.HostName), "#9CD7FF", () =>
                {
                    DrawTag(MclslRealmIds.Display(anchor.RealmIdAtAnchor), "#FFD37A");
                    GUILayout.Label("锚点年份：" + anchor.Year + "年");
                });
        }

        DrawPageHeader("回溯记录", "");
        if (_snapshot.HuanzhenHistoriesSorted.Count == 0) GUILayout.Label("尚无还真发动记录。");
        else
        {
            foreach (MclslHuanzhenHistoryRecord history in _snapshot.HuanzhenHistoriesSorted)
                DrawInfoCard(history.DeathYear + "年身死｜" + Blank(history.HostName), "#B7A7FF", () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawTag(MclslRealmIds.Display(history.RealmId), "#9CD7FF");
                    DrawTag("避环 " + history.LoopDepth, "#B7A7FF");
                    DrawTag("回到 " + (history.AnchorYear < 0 ? "无锚点" : history.AnchorYear + "年"), "#FFD37A");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label(history.Result);
                });
        }
    }

    private static void DrawResourceRules()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current ?? new MclslWorldRunState();
        DrawPageHeader("资源流转", "灵石与贡献值是修士个人修行资源，随年度修行、遗迹探索和高境界争夺自然结算。");
        GUILayout.BeginHorizontal();
        DrawOverviewPill("灵石", "修行货币", "#FFD37A", GUILayout.Width(170));
        DrawOverviewPill("贡献值", "行为货币", "#9CD7FF", GUILayout.Width(170));
        DrawOverviewPill("用途", "购买资粮", "#B7A7FF", GUILayout.Width(170));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        DrawInfoCard("来源闭环", "#FFD37A", () =>
        {
            DrawFlowRow("年度俸禄", "按境界、城市/国家归属发放少量灵石与贡献值。", "#CFC7B2");
            DrawFlowRow("背景委托", "万仙盟偏贡献与线索，五老会偏灵石、遗迹阅历与天地扰动。", "#9CD7FF");
            DrawFlowRow("势力压力", "影响过高时，引发压制、暗流、遗迹与天地异动。", "#B7A7FF");
            DrawFlowRow("宗门遗迹", "生还者可得贡献、灵石、功法残篇、筑基线索、洞天残图。", "#A7E08A");
            DrawFlowRow("炼心法门", "炼心进度可补足入门条件，心境贯穿悟法、洞天、抽髓和逆理。", "#D8C778");
            DrawFlowRow("洗瘴池", "凡人炼心较深或心境较高时有极低概率洗去凡浊入门。", "#7CCFD0");
            DrawFlowRow("洞天炼化", "金丹夺得天地之精并成就元婴时获得灵石与贡献。", "#9CD7FF");
            DrawFlowRow("天地之变", "元婴抽得天地之髓并成就化神时获得更高资源。", "#B7A7FF");
        });
        DrawInfoCard("消耗闭环", "#B7A7FF", () =>
        {
            DrawFlowRow("炼气", "炼气灵丹、筑基奇物线索。", "#9CD7FF");
            DrawFlowRow("筑基", "功法注解，提高悟法能力。", "#A7E08A");
            DrawFlowRow("金丹", "洞天残图，提高洞天争夺强度。", "#FFD37A");
            DrawFlowRow("元婴", "护髓法器，提高抽取天地之髓时的争夺强度。", "#D8C778");
            DrawFlowRow("化神以上", "天地异变密录、逆理注疏，用于炼心、履历与逆理推进。", "#B7A7FF");
        });
        DrawInfoCard("资源定位", "#9CD7FF", () =>
        {
            GUILayout.BeginHorizontal();
            DrawMiniStat("灵石", "个人修行消耗", "#FFD37A", GUILayout.Width(240));
            DrawMiniStat("贡献值", "行为与排序货币", "#9CD7FF", GUILayout.Width(260));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        });
    }

    private static void Header(string text) => Header(text, string.Empty);
    private static void Header(string text, string iconPath)
    {
        DrawIconLine(iconPath, "<b>── " + text + " ──</b>");
        GUILayout.Space(6);
    }

    private static void DrawIconLine(string iconPath, string text)
    {
        GUILayout.BeginHorizontal();
        Texture2D texture = Icon(iconPath);
        if (texture != null) GUILayout.Label(texture, GUILayout.Width(24), GUILayout.Height(24));
        GUILayout.Label(text);
        GUILayout.EndHorizontal();
    }

    private static void Label(string key, string value) => Text(key + "：" + value);
    private static void Text(string value) => GUILayout.Label(value, _labelStyle ?? GUI.skin.label);
    private static string Quality(int q) => q switch { 4 => "玄奇", 3 => "天奇", 2 => "地奇", _ => "凡奇" };
    private static string QualityColor(int q) => q switch { >= 4 => "#FF5A5A", 3 => "#FFD37A", 2 => "#9CD7FF", _ => "#CFC7B2" };
    private static string LineageColor(string state) => state switch
    {
        "道统鼎盛" => "#FFD37A",
        "传承兴起" => "#A7E08A",
        "法脉流传" => "#9CD7FF",
        "遗法复现" => "#B7A7FF",
        "秘境道统" => "#B7A7FF",
        "遗府私传" => "#D8C778",
        "遗府留痕" => "#CFC7B2",
        "道统断绝" => "#FF8877",
        _ => "#B8B8B8"
    };

    private static int AncientLineageStrength(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return 0;
        if (lineage.Completeness > 0) return Math.Clamp(lineage.Completeness, 1, 100);
        int peak = Math.Clamp(lineage.PeakPractitioners * 4, 0, 55);
        int realm = Math.Max(0, MclslRealmIds.Index(lineage.PeakRealm)) * 9;
        int cap = Math.Max(0, MclslRealmIds.Index(lineage.MaxRealm)) * 5;
        return Math.Clamp(peak + realm + cap, 1, 100);
    }

    private static int AncientLineageScore(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return 0;
        int strength = AncientLineageStrength(lineage);
        int current = Math.Clamp(lineage.CurrentPractitioners, 0, 999);
        int realm = Math.Max(0, MclslRealmIds.Index(lineage.MaxRealm));
        int branches = Math.Clamp(lineage.BranchCount, 0, 9);
        return Math.Max(100, strength * 780 + current * 120 + realm * 1800 + branches * 950);
    }

    private static string AncientLineageStrengthLabel(MclslTechniqueLineageRecord lineage)
    {
        int value = AncientLineageStrength(lineage);
        if (value >= 86) return "道统鼎盛";
        if (value >= 66) return "名动诸国";
        if (value >= 46) return "一脉成势";
        if (value >= 24) return "渐成气候";
        return "微末初传";
    }

    private static string AncientLineageState(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return "未定";
        if (lineage.CurrentPractitioners > 0) return "流传";
        if (lineage.LostYear > 0) return "沉寂";
        return string.IsNullOrWhiteSpace(lineage.State) ? "流传" : lineage.State.Replace("后世", string.Empty);
    }

    private static bool IsRecoveredAncientLineage(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return false;
        if (!string.IsNullOrWhiteSpace(lineage.LinkedRuinId)) return true;
        if (lineage.RevivedYear > 0) return true;
        return string.Equals(lineage.LifecycleState, "遗法复现", StringComparison.Ordinal)
            || string.Equals(lineage.LifecycleState, "遗府留痕", StringComparison.Ordinal)
            || string.Equals(lineage.LifecycleState, "遗府私传", StringComparison.Ordinal)
            || string.Equals(lineage.LifecycleState, "秘境道统", StringComparison.Ordinal);
    }

    private static string AncientLineageTitle(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return "无名传承";
        if (!string.IsNullOrWhiteSpace(lineage.SectDisplayName)
            && !string.Equals(lineage.SectDisplayName, Blank(lineage.Name) + "道统", StringComparison.Ordinal))
            return lineage.SectDisplayName;
        string root = Blank(lineage.Name).Trim('《', '》');
        if (string.Equals(lineage.LifecycleState, "遗府私传", StringComparison.Ordinal)) return root + "私法";
        if (string.Equals(lineage.LifecycleState, "秘境道统", StringComparison.Ordinal)) return root + "道统";
        return string.IsNullOrWhiteSpace(lineage.SectDisplayName) ? root + "道统" : lineage.SectDisplayName;
    }

    private static string AncientLineageDisplayMaxRealm(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return MclslRealmIds.LianQi;
        if (IsRecoveredAncientLineage(lineage) && MclslRealmIds.Index(lineage.MaxRealm) < MclslRealmIds.Index(MclslRealmIds.JinDan))
            return MclslRealmIds.JinDan;
        return NormalizeRealm(lineage.MaxRealm);
    }

    private static string AncientLineageRecoveredForm(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return "旧卷残存";
        if (string.Equals(lineage.LifecycleState, "秘境道统", StringComparison.Ordinal)) return "秘境道统";
        if (string.Equals(lineage.LifecycleState, "遗府私传", StringComparison.Ordinal)) return "遗府私法";
        if (lineage.RevivedYear > 0) return "残卷复现";
        return "旧脉留痕";
    }

    private static string AncientLineageArchiveGrade(MclslTechniqueLineageRecord lineage)
    {
        int strength = AncientLineageStrength(lineage);
        if (strength >= 86) return "近乎全本";
        if (strength >= 66) return "篇章完整";
        if (strength >= 46) return "可续残篇";
        if (strength >= 24) return "残文可辨";
        return "残缺难明";
    }

    private static string AncientLineageEraSourceText(MclslTechniqueLineageRecord lineage, string lineageTitle)
    {
        if (lineage == null) return "来历未详。";
        string founder = BlankFounder(lineage.FounderName);
        string year = lineage.FirstSeenYear > 0 ? lineage.FirstSeenYear + "年" : "某年";
        string title = string.IsNullOrWhiteSpace(lineageTitle) ? "此脉" : lineageTitle;
        string laws = ReplaceTags(lineage.LawTags);
        if (string.Equals(founder, "未详", StringComparison.Ordinal))
            return "此法初见于" + year + "，源头未详；只知其法义多循" + laws + "而行，后由修士口耳相授。";
        return "此法由" + founder + "传出，初见于" + year + "；或得于游历，或悟自山河，后渐归入" + title + "。";
    }

    private static string AncientLineageRecoveredSourceText(MclslTechniqueLineageRecord lineage, string lineageTitle)
    {
        if (lineage == null) return "残卷来历未详。";
        string founder = BlankFounder(lineage.FounderName);
        string title = string.IsNullOrWhiteSpace(lineageTitle) ? "此脉" : lineageTitle;
        string year = lineage.FirstSeenYear > 0 ? lineage.FirstSeenYear + "年" : "旧年";
        string revived = lineage.RevivedYear > 0 ? lineage.RevivedYear + "年" : "近年";
        bool secret = string.Equals(lineage.LifecycleState, "秘境道统", StringComparison.Ordinal);
        bool privateLine = string.Equals(lineage.LifecycleState, "遗府私传", StringComparison.Ordinal);
        string source = secret ? "旧日宗门秘境" : privateLine ? "前人遗府私藏" : "遗府残卷";
        if (string.Equals(founder, "未详", StringComparison.Ordinal))
            return "此法旧年曾见于" + year + "，后散佚于" + source + "；" + revived + "复被修士拾得，旧义未必完整。";
        return "此法旧传自" + founder + "，后随" + title + "沉入" + source + "；" + revived + "残卷再现，仍有缺文待补。";
    }

    private static string AncientLineageCurrentText(MclslTechniqueLineageRecord lineage, string lineageTitle)
    {
        if (lineage == null) return "暂无流传。";
        string title = string.IsNullOrWhiteSpace(lineageTitle) ? "此脉" : lineageTitle;
        if (lineage.CurrentPractitioners > 0)
            return title + "当世尚有" + lineage.CurrentPractitioners + "人修持，最近一次入录为" + (lineage.LastSeenYear > 0 ? lineage.LastSeenYear + "年" : "近年") + "。";
        if (lineage.LostYear > 0)
            return title + "已暂归沉寂，末次入录为" + (lineage.LastSeenYear > 0 ? lineage.LastSeenYear + "年" : lineage.LostYear + "年") + "。";
        return title + "方兴未艾，尚待承法者接续。";
    }

    private static string AncientLineageMandateText(MclslTechniqueLineageRecord lineage, int strength)
    {
        if (lineage == null) return "命数未明。";
        string state = AncientLineageStrengthLabel(lineage);
        string fate = strength >= 86 ? "一时显学" : strength >= 66 ? "诸国闻名" : strength >= 46 ? "渐入正流" : strength >= 24 ? "尚可延续" : "初见端倪";
        return state + "，" + fate + "。残卷复现后已有脉络可循，仍需承法者续补。";
    }

    private static string AncientLineageTendencyText(MclslTechniqueLineageRecord lineage, string laws)
    {
        if (lineage == null) return "未录。";
        string realm = MclslRealmIds.Display(AncientLineageDisplayMaxRealm(lineage));
        string lawText = string.IsNullOrWhiteSpace(laws) || laws == "无" ? "本脉所载道意" : laws;
        string stability = AncientLineageStrength(lineage) >= 66 ? "法脉已稳" : AncientLineageStrength(lineage) >= 36 ? "尚可承续" : "根脚未深";
        return "以" + lawText + "为本，可修至" + realm + "；" + stability + "，后续强弱仍看修士悟性与承法人数。";
    }

    private static string AncientLineageTermsText(MclslTechniqueLineageRecord lineage, string laws)
    {
        if (lineage == null) return "无";
        string realm = MclslRealmIds.Display(AncientLineageDisplayMaxRealm(lineage));
        string stability = AncientLineageStrength(lineage) >= 66 ? "道基稳固" : AncientLineageStrength(lineage) >= 36 ? "根基渐成" : "初传未稳";
        string state = AncientLineageState(lineage);
        return laws + " · 上限" + realm + " · " + stability + " · " + state;
    }

    private static int CountLineageLaws(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null || string.IsNullOrWhiteSpace(lineage.LawTags)) return 0;
        string[] parts = lineage.LawTags.Split(new[] { ',', '、' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length;
    }

    private static string BlankFounder(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "未详";
        return string.Equals(value, "上古散修", StringComparison.Ordinal) ? "未详" : value;
    }

    private static string LineageChainText(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null) return "无";
        string text = lineage.FirstSeenYear > 0 ? lineage.FirstSeenYear + "年初传" : "初传";
        if (lineage.LostYear > 0) text += " → " + lineage.LostYear + "年断绝";
        if (!string.IsNullOrWhiteSpace(lineage.LinkedRuinId)) text += string.Equals(lineage.LifecycleState, "秘境道统", StringComparison.Ordinal) ? " → 秘境留传" : " → 遗府私藏";
        if (lineage.RevivedYear > 0) text += " → " + lineage.RevivedYear + "年复现";
        if (string.Equals(lineage.State, "后世名法", StringComparison.Ordinal)) text += " → 后继有名";
        return text;
    }

    private static string RuinChainText(MclslSectRuinRecord ruin, string lineageName, string sectName)
    {
        if (ruin == null) return "无";
        string title = string.IsNullOrWhiteSpace(sectName) ? "无名道统" : sectName;
        string technique = string.IsNullOrWhiteSpace(lineageName) ? Blank(ruin.SourceTechniqueName) : lineageName;
        string text = title + "《" + technique + "》";
        if (ruin.SourceTechniqueLostYear > 0) text += " → " + ruin.SourceTechniqueLostYear + "年断绝";
        text += " → " + Blank(ruin.Name);
        if (ruin.SourceTechniqueRevivedYear > 0) text += " → " + ruin.SourceTechniqueRevivedYear + "年复现";
        return text;
    }

    private static string DisplayHolder(MclslGeneratedItemRecord item) => string.IsNullOrWhiteSpace(item.HolderName) ? "无主" : item.HolderName;
    private static string RootText(MclslGeneratedItemRecord item) => item == null ? "无" : !string.IsNullOrWhiteSpace(item.AttributeText) ? item.AttributeText : MclslGeneratedObjectFactory.RootText(item.Kind, MclslGeneratedObjectFactory.SplitTags(item.LawTags), item.Quality);
    private static string ReplaceTags(string value) => string.IsNullOrWhiteSpace(value) ? "无" : value.Replace(",", "、");
    private static string Blank(string value) => string.IsNullOrWhiteSpace(value) ? "无" : value;
    private static string PlayerFacingEventBody(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "无详载。";
        return value
            .Replace("WorldBox 原生逻辑", "世俗王朝之势")
            .Replace("WorldBox 原生", "世俗")
            .Replace("原生国家", "凡俗国度")
            .Replace("原生世界", "人间")
            .Replace("原生地形", "地脉形貌")
            .Replace("不创建新的国家或地图势力", "不立新国")
            .Replace("不改变当地原生国家归属", "不改当地归属")
            .Replace("灵根仙道延续千年，灵根、功法、真元、神魂与大道感悟构成旧修行秩序；天地将变，上古仙道至此入终纪。", "千年仙道，至此风雨满楼。")
            .Replace("上古仙道纪，", "仙道纪元，")
            .Replace("在上古仙道纪", "于仙道纪元")
            .Replace("上古仙道仍在自然流转", "仙道仍在自然流转")
            .Replace("传法天尊逆天地之理，开创新法。自此众生无须天赐灵根，亦可炼气登仙。", "传法立道，天下始闻新法。")
            .Replace("万仙盟依传法新法建立跨国背景秩序，提供标准功法、贡献体系、遗迹任务与修行资源调配，但不取代凡俗国度。", "万仙盟传檄诸国，新法自此有统。")
            .Replace("万仙盟依传法新法建立跨国背景秩序，提供标准功法、贡献体系、遗迹任务与修行资源调配，但不取代原生国家。", "万仙盟传檄诸国，新法自此有统。")
            .Replace("变世之后，旧法修士不会自动补成新法修士。旧法遗修保留境界与寿元，但不再使用筑基奇物、洞天、天地之变与天地之魄。", "前代仙修或隐或散，山河间旧影渐稀。")
            .Replace("五位长生天尊先后逆天地之理，结成五老会。第五席逆理暂未揭示，只作为原著背景席位记录。", "五老同席，暗潮入世。")
            .Replace("传法变世十年后，旧秩序基本崩解。万仙盟仍为第一大背景势力，五老会自暗处开始与其长期争衡。", "旧山河远去，新法定世。")
            .Replace("人间大战死伤汇聚，同年已记录20次相关死亡，天地法则由此激变。", string.Empty)
            .Replace("天地法则由此激变。", string.Empty)
            .Replace("魄核归属开始判定。", string.Empty)
            .Replace("现世五年而无人开启争夺，魄核未被祭炼，遂散入天地，待后世再显。", "隐没于天地之间。")
            .Replace("旧法时期，", "仙道纪元，")
            .Replace("旧法修士只能观其天职以印证自身之道，不能攻击、祭炼或借最后一击合道。", "仙修由此印证自身大道。")
            .Replace("此时修士只能遥观其天职", "高境仙修遥观其天职")
            .Replace("，不能攻击、祭炼或借最后一击合道", string.Empty)
            .Replace("不能祭炼夺魄", "未得承载之法");
    }
    private static int CaveStateOrder(string state) => state switch { "活跃" => 0, "衰退" => 1, "枯竭" => 2, _ => 3 };
    private static int WorldChangeStateOrder(string state) => state switch { "活跃" => 0, "衰减" => 1, "平息" => 2, _ => 3 };
    private static int RuinStateOrder(string state) => state switch { "显世" => 0, "探索中" => 1, "搜尽" => 2, "崩毁" => 3, _ => 4 };
    private static int SoulStateOrder(string state) => state switch { "显化" => 0, "已祭炼" => 1, "沉寂" => 2, "散逸" => 3, _ => 4 };
    private static bool IsRuinOpen(MclslSectRuinRecord ruin) => ruin != null && ruin.RemainingValue > 0 && ruin.State != "搜尽" && ruin.State != "封绝";
    private static void DrawFlowRow(string title, string body, string color)
    {
        GUILayout.BeginHorizontal(GUI.skin.box);
        DrawTag(title, color);
        GUILayout.Label(body);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private static void DrawFactionRows(IReadOnlyList<MclslFactionMissionRecord> missions, IReadOnlyList<MclslFactionPressureRecord> pressures, string color)
    {
        int shown = 0;
        if (missions != null)
        {
            foreach (MclslFactionMissionRecord mission in missions)
            {
                if (shown >= 5) return;
                DrawFactionMissionRow(mission, color);
                shown++;
            }
        }
        if (pressures != null)
        {
            foreach (MclslFactionPressureRecord pressure in pressures)
            {
                if (shown >= 5) return;
                DrawFactionPressureRow(pressure, color);
                shown++;
            }
        }
        if (shown == 0) GUILayout.Label("近期无事入录。");
    }

    private static void DrawFactionMissionRow(MclslFactionMissionRecord mission, string color)
    {
        if (mission == null) return;
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();
        DrawTag(mission.Year + "年", "#CFC7B2");
        DrawTag(mission.MissionName, color);
        DrawTag(Blank(mission.ActorName), "#FFD37A");
        if (mission.ContributionReward > 0) DrawTag("贡献+" + mission.ContributionReward, "#9CD7FF");
        if (mission.SpiritStoneReward > 0) DrawTag("灵石+" + mission.SpiritStoneReward, "#FFD37A");
        if (mission.InfluenceDelta != 0) DrawTag("影响" + Signed(mission.InfluenceDelta), mission.InfluenceDelta > 0 ? "#A7E08A" : "#FF8877");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (!string.IsNullOrWhiteSpace(mission.Summary)) GUILayout.Label(PlayerFacingEventBody(mission.Summary));
        GUILayout.EndVertical();
    }

    private static void DrawFactionPressureRow(MclslFactionPressureRecord pressure, string color)
    {
        if (pressure == null) return;
        GUILayout.BeginHorizontal(GUI.skin.box);
        DrawTag(pressure.Year + "年", "#CFC7B2");
        DrawTag(pressure.PolicyName, color);
        DrawTag("压力" + Signed(pressure.PressureDelta), pressure.PressureDelta > 0 ? "#FF8877" : "#A7E08A");
        GUILayout.Label(PlayerFacingEventBody(string.IsNullOrWhiteSpace(pressure.WorldEffect) ? pressure.Summary : pressure.WorldEffect));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();

    private static string FirstSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "记录";
        int index = value.IndexOfAny(new[] { '｜', '：', ':' });
        return index > 0 ? value.Substring(0, Math.Min(index, 24)) : value.Substring(0, Math.Min(value.Length, 24));
    }

    private static void DrawPageHeader(string title, string subtitle)
    {
        GUILayout.Label("<size=22><b>" + title + "</b></size>");
        if (!string.IsNullOrWhiteSpace(subtitle))
            GUILayout.Label("<color=#B9B0A0>" + subtitle + "</color>");
        GUILayout.Space(8);
    }

    private static string EraDisplay(MclslWorldRunState run)
    {
        string era = run?.CultivationEpoch ?? string.Empty;
        return era switch
        {
            MclslWorldEpochSystem.AncientLawEpoch => "仙道纪元",
            MclslWorldEpochSystem.TransmissionTransitionEpoch => "传法变世",
            MclslWorldEpochSystem.NewLawEpoch => "新法纪元",
            _ => "仙道纪元"
        };
    }

    private static string EraRuleText(MclslWorldRunState run)
    {
        if (run == null) return "仙道未尽";
        List<string> rules = new();
        if (MclslNewLawPioneerSystem.IsPioneerEra(MclslRuntime.CurrentYear()))
            rules.Add("新法初传·累计先行" + Math.Max(0, run.NewLawPioneerCount) + "人");
        else if (run.NewLawEnabled) rules.Add("新法已传");
        if (run.LawConflictEnabled) rules.Add("法不可同修");
        if (run.ImmortalMortalMiasmaEnabled) rules.Add("仙凡瘴");
        if (run.FiveEldersFounded) rules.Add("五老会已立");
        return rules.Count == 0 ? "灵根仙道" : string.Join("｜", rules);
    }

    private static string EraDescription(MclslWorldRunState run)
    {
        if (run == null) return "本世仍处仙道纪元。";
        return run.CultivationEpoch switch
        {
            MclslWorldEpochSystem.AncientLawEpoch => MclslNewLawPioneerSystem.IsPioneerEra(MclslRuntime.CurrentYear())
                ? "旧仙道仍是天下主流，但少数先行修士已在传法天尊证道前试行新法；此时尚无仙法不可同修、仙凡瘴、万仙盟与五老会。"
                : "灵根仙道依赖灵根、功法、真元、神魂与大道感悟；此纪元只展示仙道自身的事件与档案。",
            MclslWorldEpochSystem.TransmissionTransitionEpoch => "传法变世正在推进：早期新法由少数先行者扩为天下法门，法不可同修、仙凡瘴与万仙盟秩序会按年落地；旧法修士将按自身要素尝试转修。",
            MclslWorldEpochSystem.NewLawEpoch => "新法纪元已经稳定：万仙盟维持新法秩序，五老会从暗处渗透，两大背景势力不取代凡俗国度；旧体系被后世称为古法。",
            _ => "本世时代状态尚未归档。"
        };
    }

    private static string YearLabel(int year) => year > 0 ? year + "年" : "未定";

    private static MclslCodexTab[] ActiveTabs()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return MclslCodexTabCatalog.ForEpoch(run?.CultivationEpoch);
    }

    private void DrawRuntimeHealthCard(MclslWorldRunState run)
    {
        int actorBacklog = Math.Max(_snapshot.AnnualActorBacklog, _snapshot.AnnualActorStates);
        int moduleBacklog = _snapshot.AnnualModuleBacklog + _snapshot.LoadRecoveryBacklog;
        bool busy = actorBacklog > 0 || moduleBacklog > 0 || _snapshot.AnnualWorldWorkPending;
        DrawInfoCard("运行健康", busy ? "#FFD37A" : "#A7E08A", () =>
        {
            GUILayout.BeginHorizontal();
            DrawMiniStat("档案版本", "v" + _snapshot.ArchiveVersion, "#CFC7B2", GUILayout.Width(145));
            DrawMiniStat("追踪角色", _snapshot.TrackedActors.ToString(), "#9CD7FF", GUILayout.Width(145));
            DrawMiniStat("年度候选", _snapshot.AnnualCandidates.ToString(), "#FFD37A", GUILayout.Width(145));
            DrawMiniStat("角色队列", actorBacklog.ToString(), actorBacklog > 2048 ? "#FF8877" : "#A7E08A", GUILayout.Width(145));
            DrawMiniStat("模块队列", moduleBacklog.ToString(), moduleBacklog > 0 ? "#FFD37A" : "#A7E08A", GUILayout.Width(145));
            DrawMiniStat("世界结算", _snapshot.AnnualWorldWorkPending ? "处理中" : "空闲", _snapshot.AnnualWorldWorkPending ? "#FFD37A" : "#A7E08A", GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("档案记录：纪事 " + (run?.Events?.Count ?? 0) + "｜死亡 " + (run?.DeathRecords?.Count ?? 0)
                + "｜功法传承 " + (run?.TechniqueLineages?.Count ?? 0) + "。此处只读取现有计数，不新增世界扫描。");
        });
    }

    private static void DrawOverviewPill(string label, string value, string color, params GUILayoutOption[] options)
    {
        GUILayout.BeginVertical(GUI.skin.box, options);
        GUILayout.Label("<color=grey>" + label + "</color>");
        GUILayout.Label("<size=22><b><color=" + color + ">" + value + "</color></b></size>");
        GUILayout.EndVertical();
    }

    private static void DrawMiniStat(string label, string value, string color, params GUILayoutOption[] options)
    {
        GUILayout.BeginVertical(options);
        GUILayout.Label("<color=grey>" + label + "</color>");
        GUILayout.Label("<b><color=" + color + ">" + value + "</color></b>");
        GUILayout.EndVertical();
    }

    private static void DrawInfoCard(string title, string accent, Action content)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        Rect stripe = GUILayoutUtility.GetRect(100f, 5f, GUILayout.ExpandWidth(true));
        DrawSolidRect(stripe, ParseHexColor(accent, Color.gray));
        GUILayout.Label("<size=20><b>" + title + "</b></size>");
        content?.Invoke();
        GUILayout.EndVertical();
        GUILayout.Space(4);
    }

    private static void DrawItemCard(string title, string quality, string tags, string holder, string origin, string root, string description, string accent)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        Rect stripe = GUILayoutUtility.GetRect(100f, 5f, GUILayout.ExpandWidth(true));
        DrawSolidRect(stripe, ParseHexColor(accent, Color.yellow));
        GUILayout.BeginHorizontal();
        GUILayout.Label("<size=20><b>" + title + "</b></size>", GUILayout.Width(360));
        DrawTag(quality, accent);
        DrawTag(tags, "#9CD7FF");
        DrawTag("持有 " + holder, "#CFC7B2");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (!string.IsNullOrWhiteSpace(root)) GUILayout.Label("根基作用：<color=#FFD37A>" + root + "</color>");
        if (!string.IsNullOrWhiteSpace(origin)) GUILayout.Label("来历：<color=#B9B0A0>" + origin + "</color>");
        if (!string.IsNullOrWhiteSpace(description)) GUILayout.Label(description);
        GUILayout.EndVertical();
        GUILayout.Space(3);
    }

    private static void DrawTag(string text, string color)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        GUILayout.Label("<b><color=" + color + ">" + text + "</color></b>", _tagStyle ?? GUI.skin.box, GUILayout.Height(28));
    }

    private static void DrawSolidRect(Rect rect, Color color)
    {
        Texture2D texture = _whiteTexture ?? SolidTexture(Color.white);
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, texture);
        GUI.color = old;
    }

    private static Color ParseHexColor(string hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return fallback;
        if (!hex.StartsWith("#", StringComparison.Ordinal)) hex = "#" + hex;
        return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : fallback;
    }

    private static void EnsureStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;
        _whiteTexture = SolidTexture(Color.white);
        _windowBackground = _whiteTexture;
        _backdropTexture = _whiteTexture;
        _windowStyle = new GUIStyle(GUI.skin.window)
        {
            fontSize = 24
        };
        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18
        };
        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            wordWrap = true,
            richText = true,
            normal = { textColor = new Color(0.9f, 0.94f, 0.91f) }
        };
        _tagStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 18,
            richText = true,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(8, 8, 4, 4)
        };
    }

    private static Rect FitRect()
    {
        float maxWidth = Math.Max(960f, Screen.width - 80f);
        float maxHeight = Math.Max(720f, Screen.height - 80f);
        float width = Math.Min(1600f, maxWidth);
        float height = Math.Min(1230f, maxHeight);
        float x = Math.Clamp(60f, 20f, Math.Max(20f, Screen.width - width - 20f));
        float y = Math.Clamp(60f, 20f, Math.Max(20f, Screen.height - height - 20f));
        return new Rect(x, y, width, height);
    }

    private void DrawBackdrop()
    {
        if (_whiteTexture == null) _whiteTexture = SolidTexture(Color.white);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _whiteTexture,
            ScaleMode.StretchToFill, true, 0, new Color(0f, 0f, 0f, 0.72f), 0, 0);
        GUI.DrawTexture(_rect, _whiteTexture,
            ScaleMode.StretchToFill, true, 0, new Color(0.035f, 0.032f, 0.028f, 0.98f), 0, 0);
    }

    private static void CreateOverlayBlocker()
    {
        if (_overlayBlocker != null) return;
        try
        {
            if (_whiteTexture == null) _whiteTexture = SolidTexture(Color.white);
            _overlayBlocker = new GameObject("MclslCodexOverlay");
            Canvas canvas = _overlayBlocker.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 29990;
            _overlayBlocker.AddComponent<GraphicRaycaster>();
            Image image = _overlayBlocker.AddComponent<Image>();
            image.sprite = Sprite.Create(_whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;
            RectTransform rect = _overlayBlocker.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            DontDestroyOnLoad(_overlayBlocker);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslCodexWindow-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslCodexWindow.cs #2: " + mclslEmptyCatchEx.Message); }
    }

    private static void DestroyOverlayBlocker()
    {
        if (_overlayBlocker == null) return;
        try { Destroy(_overlayBlocker); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslCodexWindow-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslCodexWindow.cs #3: " + mclslEmptyCatchEx.Message); }
        _overlayBlocker = null;
    }

    private static Texture2D SolidTexture(Color color)
    {
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Texture2D Icon(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (IconCache.TryGetValue(path, out Texture2D cached)) return cached;
        Sprite sprite = null;
        try { sprite = SpriteTextureLoader.getSprite(path); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslCodexWindow-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslCodexWindow.cs #4: " + mclslEmptyCatchEx.Message); }
        Texture2D texture = sprite?.texture;
        IconCache[path] = texture;
        return texture;
    }

}
