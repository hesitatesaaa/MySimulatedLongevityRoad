using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private sealed class AtlasCity
    {
        internal long Id;
        internal string Name = string.Empty;
        internal int X = -1, Y = -1;
        internal MclslFamilyRecord Owner;
        internal string Group = string.Empty;
        internal Color OwnerColor = new(0.58f, 0.61f, 0.65f);
        internal GUIContent MarkerTooltip;
        internal readonly List<(MclslFamilyRecord Family, int Score)> Families = new();
    }

    private static readonly Color[] AtlasSectColors =
    {
        AtlasColor("#167F69"), AtlasColor("#2457A6"),
        AtlasColor("#9A6214"), AtlasColor("#66358C"),
        AtlasColor("#176F8F"), AtlasColor("#A34724"),
        AtlasColor("#57636F"), AtlasColor("#3F7826"),
        AtlasColor("#963365"), AtlasColor("#765226"),
        AtlasColor("#9C8218"), AtlasColor("#8F2834")
    };

    private static Color AtlasColor(string html)
        => ColorUtility.TryParseHtmlString(html, out Color color) ? color : Color.gray;

    private readonly List<AtlasCity> _atlasCities = new();
    private readonly List<(string Name, Color Color)> _atlasGroups = new();
    private int _atlasPositioned;
    private long _atlasRevision = -1;
    private long _atlasAffiliationRevision = -1;
    private bool _atlasNewLaw;
    private MapBox _atlasWorld;
    private long _atlasSelectedCityId;

    private void DrawWorldAtlasOverview()
    {
        EnsureAtlasCities();
        DrawPageHeader(LM.Get("mclsl_atlas_title"), LM.Get("mclsl_atlas_description"));
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.Label("宗门疆域 · 城镇归属", GUILayout.ExpandWidth(true));
        if (GUILayout.Button("刷新舆图", GUILayout.Width(108f)))
        {
            _atlasRevision = -1;
            EnsureAtlasCities();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("城镇颜色随影响力主家所属势力确定；灰色表示尚无明确归属。");
        GUILayout.EndVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("可绘城镇  " + _atlasPositioned + " / " + _atlasCities.Count, GUI.skin.box, GUILayout.Width(185f));
        GUILayout.Label("已识别势力  " + _atlasGroups.Count, GUI.skin.box, GUILayout.Width(185f));
        GUILayout.EndHorizontal();
        GUILayout.BeginVertical(GUI.skin.box);
        int groupColumn = 0;
        foreach (var group in _atlasGroups)
        {
            if (groupColumn % 5 == 0) GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#" + ColorUtility.ToHtmlStringRGB(group.Color)
                + ">◆</color>  " + group.Name, GUILayout.Width(170f));
            groupColumn++;
            if (groupColumn % 5 == 0) GUILayout.EndHorizontal();
        }
        if (groupColumn % 5 != 0) GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        if (_atlasCities.Count == 0) { GUILayout.Label(LM.Get("mclsl_atlas_no_cities")); return; }
        Texture2D texture = World.world?.world_layer?.texture;
        if (texture == null || MapBox.width <= 0 || MapBox.height <= 0 || _atlasPositioned == 0)
        {
            GUILayout.Label(LM.Get("mclsl_atlas_no_map"));
            DrawAtlasCityList(_atlasCities);
            DrawAtlasSelectedCity();
            return;
        }
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DrawAtlasCanvas(texture);
        GUILayout.EndVertical();
        GUILayout.Space(8f);
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Mathf.Min(300f, _rect.width * 0.24f)));
        DrawAtlasSelectedCity();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        if (_atlasPositioned < _atlasCities.Count)
        {
            GUILayout.Label("未定位城镇");
            DrawAtlasCityList(_atlasCities.Where(x => x.X < 0 || x.Y < 0));
        }
    }

    private void EnsureAtlasCities()
    {
        MapBox world = World.world;
        long revision = MclslWorldArchiveStore.Revision;
        bool newLaw = MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear());
        long affiliationRevision = MclslRankSnapshotSource.Revision;
        if (ReferenceEquals(_atlasWorld, world) && _atlasRevision == revision)
        {
            if (_atlasAffiliationRevision != affiliationRevision || _atlasNewLaw != newLaw)
            {
                foreach (AtlasCity city in _atlasCities)
                {
                    if (city.Owner == null) continue;
                    city.Group = AtlasAffiliation(city.Owner);
                    city.OwnerColor = AtlasFamilyColor(city.Owner, city.Group);
                }
                _atlasAffiliationRevision = affiliationRevision;
                _atlasNewLaw = newLaw;
                RefreshAtlasSummary();
            }
            return;
        }
        _atlasWorld = world;
        _atlasRevision = revision;
        _atlasAffiliationRevision = affiliationRevision;
        _atlasNewLaw = newLaw;
        _atlasCities.Clear();
        _atlasGroups.Clear();
        _atlasPositioned = 0;
        if (world?.cities == null) return;
        IReadOnlyList<MclslFamilyRecord> families = MclslFamilySystem.Families;
        foreach (City city in world.cities)
        {
            if (city?.data == null) continue;
            AtlasCity item = new() { Id = city.data.id, Name = string.IsNullOrWhiteSpace(city.data.name) ? "未名城镇" : city.data.name };
            if (TryAtlasPosition(city, 0, out Vector2 point))
            {
                item.X = Mathf.Clamp(Mathf.RoundToInt(point.x), 0, Math.Max(0, MapBox.width - 1));
                item.Y = Mathf.Clamp(Mathf.RoundToInt(point.y), 0, Math.Max(0, MapBox.height - 1));
            }
            foreach (MclslFamilyRecord family in families)
                if (family?.CityInfluence != null && family.CityInfluence.TryGetValue(item.Id, out int score) && score > 0)
                    item.Families.Add((family, score));
            item.Families.Sort((a, b) =>
            {
                int order = b.Score.CompareTo(a.Score);
                return order != 0 ? order : string.Compare(a.Family.Id, b.Family.Id, StringComparison.Ordinal);
            });
            if (item.Families.Count > 0)
            {
                item.Owner = item.Families[0].Family;
                item.Group = AtlasAffiliation(item.Owner);
                item.OwnerColor = AtlasFamilyColor(item.Owner, item.Group);
            }
            item.MarkerTooltip = new GUIContent(string.Empty,
                item.Name + " · " + (item.Owner?.Name ?? "暂无主家"));
            _atlasCities.Add(item);
        }
        _atlasCities.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        RefreshAtlasSummary();
        if (!_atlasCities.Any(x => x.Id == _atlasSelectedCityId)) _atlasSelectedCityId = _atlasCities.Count > 0 ? _atlasCities[0].Id : 0;
    }

    private void RefreshAtlasSummary()
    {
        _atlasGroups.Clear();
        _atlasPositioned = 0;
        HashSet<string> shown = new(StringComparer.Ordinal);
        foreach (AtlasCity city in _atlasCities)
        {
            if (city.X >= 0 && city.Y >= 0) _atlasPositioned++;
            if (city.Group.Length > 0 && shown.Add(city.Group))
                _atlasGroups.Add((city.Group, city.OwnerColor));
        }
        _atlasGroups.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    private void DrawAtlasCanvas(Texture2D texture)
    {
        float detailWidth = Mathf.Min(300f, _rect.width * 0.24f);
        float availableWidth = Mathf.Max(320f, _rect.width - 218f - detailWidth - 80f);
        float height = Mathf.Clamp(availableWidth * texture.height / Mathf.Max(1f, texture.width),
            200f, Mathf.Max(300f, _rect.height * 0.8f));
        Rect canvas = GUILayoutUtility.GetRect(320f, height, GUILayout.ExpandWidth(true));
        float scale = Mathf.Min(canvas.width / Mathf.Max(1f, texture.width), canvas.height / Mathf.Max(1f, texture.height));
        Rect rect = new(canvas.x + (canvas.width - texture.width * scale) * 0.5f,
            canvas.y + (canvas.height - texture.height * scale) * 0.5f,
            texture.width * scale, texture.height * scale);
        Color previous = GUI.color;
        Color previousContent = GUI.contentColor;
        Color background = GUI.backgroundColor;
        GUI.color = Color.white;
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false);
        GUI.color = new Color(0.05f, 0.08f, 0.09f, 0.19f);
        GUI.DrawTexture(rect, _whiteTexture);
        foreach (AtlasCity city in _atlasCities)
        {
            if (city.X < 0 || city.Y < 0) continue;
            float x = rect.x + (city.X + 0.5f) / MapBox.width * rect.width;
            float y = rect.y + (1f - (city.Y + 0.5f) / MapBox.height) * rect.height;
            bool selected = city.Id == _atlasSelectedCityId;
            float size = selected ? 23f : 17f;
            Rect marker = new(x - size * 0.5f, y - size * 0.5f, size, size);
            GUI.color = new Color(city.OwnerColor.r, city.OwnerColor.g, city.OwnerColor.b, city.Owner == null ? 0.10f : 0.30f);
            GUI.DrawTexture(new Rect(x - 18f, y - 18f, 36f, 36f), _whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(marker.x - 2f, marker.y - 2f, marker.width + 4f, marker.height + 4f), _whiteTexture);
            GUI.color = city.OwnerColor;
            GUI.DrawTexture(marker, _whiteTexture);
            GUI.color = Color.white;
            if (GUI.Button(marker, city.MarkerTooltip, GUIStyle.none))
                _atlasSelectedCityId = city.Id;
            if (selected)
            {
                GUI.contentColor = city.OwnerColor;
                GUI.Label(new Rect(Mathf.Min(x + 12f, rect.xMax - 150f), y - 28f, 150f, 22f), city.Name);
                GUI.contentColor = previousContent;
            }
        }
        GUI.color = previous;
        GUI.contentColor = previousContent;
        GUI.backgroundColor = background;
    }

    private void DrawAtlasSelectedCity()
    {
        AtlasCity selected = _atlasCities.Find(x => x.Id == _atlasSelectedCityId);
        if (selected == null) { GUILayout.Label("选择城镇查看家族影响。"); return; }
        Rect accent = GUILayoutUtility.GetRect(1f, 5f, GUILayout.ExpandWidth(true));
        Color oldColor = GUI.color;
        GUI.color = selected.OwnerColor;
        GUI.DrawTexture(accent, _whiteTexture);
        GUI.color = oldColor;
        Color oldContentColor = GUI.contentColor;
        GUI.contentColor = selected.OwnerColor;
        GUILayout.Label("<b>" + selected.Name + "</b>");
        GUI.contentColor = oldContentColor;
        GUILayout.Label("城镇坐标：" + selected.X + "，" + selected.Y);
        GUILayout.Label("势力归属：" + (selected.Group.Length == 0 ? "无归属" : selected.Group));
        if (selected.Owner == null)
        {
            GUILayout.Label("暂无影响力主家；家族每五年结算一次。");
            return;
        }
        GUILayout.Label("主家（按影响力）：" + selected.Owner.Name);
        for (int i = 0; i < Math.Min(3, selected.Families.Count); i++)
        {
            var entry = selected.Families[i];
            GUILayout.Label((i == 0 ? "主家" : "第" + (i + 1) + "家") + " · " + entry.Family.Name + "　影响 " + entry.Score);
        }
        if (GUILayout.Button(LM.Get("mclsl_atlas_open_family"), GUILayout.Height(36f)))
        {
            MclslCodexTab[] tabs = ActiveTabs();
            for (int i = 0; i < tabs.Length; i++)
                if (tabs[i].Title == "修仙家族") { SelectCodexTab(i); break; }
            _selectedFamilyId = selected.Owner.Id;
            _scroll = Vector2.zero;
        }
    }

    private void DrawAtlasCityList(IEnumerable<AtlasCity> cities)
    {
        foreach (AtlasCity city in cities)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            Color previous = GUI.contentColor;
            GUI.contentColor = city.OwnerColor;
            GUILayout.Label(city.Name + "　·　" + (city.Owner?.Name ?? "暂无主家"));
            GUI.contentColor = previous;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("查看", GUILayout.Width(68f))) _atlasSelectedCityId = city.Id;
            GUILayout.EndHorizontal();
        }
    }

    private static string AtlasAffiliation(MclslFamilyRecord family)
    {
        if (family == null) return string.Empty;
        if (family.HeadActorId > 0 && MclslActorRegistry.ResolveKnownOrWorld(family.HeadActorId, out Actor head)
            && MclslActorAccessor.Alive(head))
        {
            string headGroup = AtlasActorAffiliation(head);
            if (headGroup.Length > 0) return headGroup;
        }
        int bestRealm = int.MinValue;
        long bestId = long.MaxValue;
        string bestGroup = string.Empty;
        foreach (MclslFamilyMemberRecord member in family.Members ?? new List<MclslFamilyMemberRecord>())
        {
            if (member?.Alive != true || member.ActorId <= 0
                || !MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor actor)
                || !MclslActorAccessor.Alive(actor)) continue;
            string group = AtlasActorAffiliation(actor);
            if (group.Length == 0) continue;
            int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
            if (realm > bestRealm || realm == bestRealm && member.ActorId < bestId)
            { bestRealm = realm; bestId = member.ActorId; bestGroup = group; }
        }
        return bestGroup;
    }

    private static string AtlasActorAffiliation(Actor actor)
    {
        if (!MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear()))
        {
            string sectId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientSectAffiliation, string.Empty);
            foreach (var sect in MclslFactionMissionSystem.AncientSects)
                if (sect.Id == sectId) return sect.Name;
            return string.Empty;
        }
        return MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty) switch
        {
            "wanxian" or "万仙盟" => "万仙盟",
            "five_elders" or "五老会" => "五老会",
            _ => string.Empty
        };
    }

    private static Color AtlasFamilyColor(MclslFamilyRecord family, string group)
    {
        if (string.IsNullOrWhiteSpace(group)) return new Color(0.58f, 0.61f, 0.65f);
        for (int i = 0; i < MclslFactionMissionSystem.AncientSects.Length; i++)
            if (group == MclslFactionMissionSystem.AncientSects[i].Name) return AtlasSectColors[i];
        if (group == "万仙盟") return AtlasSectColors[10];
        if (group == "五老会") return AtlasSectColors[11];
        uint hash = 2166136261;
        foreach (char c in family.Id ?? string.Empty) hash = (hash ^ c) * 16777619;
        return Color.HSVToRGB((hash % 360) / 360f, 0.48f, 0.85f);
    }

    private static bool TryAtlasPosition(object source, int depth, out Vector2 position)
    {
        position = default;
        if (source == null || depth > 3) return false;
        if (source is Vector3 v3) { position = new Vector2(v3.x, v3.y); return true; }
        if (source is Vector2 v2) { position = v2; return true; }
        Type type = source.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        object x = ReadAtlasMember(type, source, "x", flags);
        object y = ReadAtlasMember(type, source, "y", flags);
        if (x != null && y != null)
        {
            try { position = new Vector2(Convert.ToSingle(x), Convert.ToSingle(y)); return true; }
            catch { }
        }
        foreach (string member in new[] { "current_position", "position", "city_center", "center", "main_tile", "tile", "current_tile" })
        {
            object nested = ReadAtlasMember(type, source, member, flags);
            if (nested != null && !ReferenceEquals(nested, source) && TryAtlasPosition(nested, depth + 1, out position)) return true;
        }
        foreach (string method in new[] { "getTile", "getCenterTile", "getCityCenterTile" })
        {
            object nested;
            try { nested = type.GetMethod(method, flags, null, Type.EmptyTypes, null)?.Invoke(source, null); }
            catch { continue; }
            if (nested != null && !ReferenceEquals(nested, source) && TryAtlasPosition(nested, depth + 1, out position)) return true;
        }
        return false;
    }

    private static object ReadAtlasMember(Type type, object source, string name, BindingFlags flags)
    {
        try { return type.GetField(name, flags)?.GetValue(source) ?? type.GetProperty(name, flags)?.GetValue(source); }
        catch { return null; }
    }
}
