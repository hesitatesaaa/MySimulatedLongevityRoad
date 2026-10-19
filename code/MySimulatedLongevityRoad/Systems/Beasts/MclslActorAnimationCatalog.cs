using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MySimulatedLongevityRoad.Core;
using Newtonsoft.Json.Linq;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslActorAnimationCatalog
{
    internal sealed class FrameProfile
    {
        internal int Width;
        internal int Height;
        internal float AnchorX;
        internal float AnchorY;
        internal float PixelsPerUnit;
        internal bool AnchorApproved;
    }

    internal sealed class Form
    {
        internal string ResourcePath;
        internal int Width;
        internal int Height;
        internal float AnchorX;
        internal float AnchorY;
        internal float PixelsPerUnit;
        internal float Scale;
        internal string NativeResourcePath;
        internal bool AnchorApproved;
        internal bool ScaleApproved;
        internal readonly Dictionary<string, MclslAnimationClipData> Actions = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, FrameProfile> FrameProfiles = new(StringComparer.Ordinal);
    }

    internal static readonly string[] ActionNames = { "idle", "walk", "attack", "hit", "death" };
    private static readonly Dictionary<string, Form> Forms = new(StringComparer.Ordinal);
    private static bool _loaded;

    internal static string ResourceRoot => Path.Combine(
        MySimulatedLongevityRoad.MclslMod.I.GetDeclaration().FolderPath, "GameResources");
    internal static string NativeResourceRoot => Path.Combine(
        MySimulatedLongevityRoad.MclslMod.I.GetDeclaration().FolderPath, "NativeActorResources");

    internal static Form Get(string formPath)
    {
        EnsureLoaded();
        return formPath != null && Forms.TryGetValue(formPath.TrimEnd('/'), out Form form) ? form : null;
    }

    internal static MclslAnimationClipData Clip(string formPath, string action)
    {
        Form form = Get(formPath);
        return form != null && form.Actions.TryGetValue(action, out MclslAnimationClipData clip)
            ? clip : DiscoverLegacy(formPath, action);
    }

    internal static string ResolveFile(string resourcePath)
    {
        string root = Path.GetFullPath(ResourceRoot).TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string file = Path.GetFullPath(Path.Combine(root, resourcePath + ".png"));
        if (!file.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Resource escapes mod directory: " + resourcePath);
        return file;
    }

    internal static string ResolveNativeFile(string resourcePath)
    {
        string root = Path.GetFullPath(NativeResourceRoot).TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string file = Path.GetFullPath(Path.Combine(root, resourcePath + ".png"));
        if (!file.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Native frame escapes mod directory: " + resourcePath);
        return file;
    }

    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        string file = Path.Combine(MySimulatedLongevityRoad.MclslMod.I.GetDeclaration().FolderPath,
            "resources", "actor-frames.manifest.json");
        if (!File.Exists(file)) return;
        try
        {
            JObject manifest = JObject.Parse(File.ReadAllText(file));
            int version = (int?)manifest["schemaVersion"] ?? 0;
            if (version != 1 && version != 2) throw new InvalidDataException("Unknown actor manifest schema");
            if (manifest["groups"] is not JArray groups) throw new InvalidDataException("Missing actor groups");
            foreach (JToken token in groups)
            {
                try
                {
                    JObject group = (JObject)token;
                    string path = ((string)group["resourcePath"])?.TrimEnd('/');
                    if (string.IsNullOrEmpty(path)) throw new InvalidDataException("Missing resourcePath");
                    ResolveFile(path + "/main/idle_0");
                    JArray canvas = (JArray)group["canvas"], anchor = (JArray)group["anchor"];
                    if (canvas?.Count != 2 || anchor?.Count != 2) throw new InvalidDataException("Invalid canvas/anchor");
                    JObject calibration = group["calibration"] as JObject;
                    bool integrated = (bool?)group["integrationAccepted"] == true;
                    bool approved = (bool?)group["approved"] == true || integrated;
                    Form form = new()
                    {
                        ResourcePath = path, Width = (int)canvas[0], Height = (int)canvas[1],
                        AnchorX = (float)anchor[0], AnchorY = (float)anchor[1],
                        PixelsPerUnit = (float?)group["pixelsPerUnit"] ?? 0f,
                        AnchorApproved = approved && ((bool?)group["anchorReviewed"] == true
                            || integrated && (bool?)group["geometryConfigured"] == true),
                        // Preview calibration may be exercised before the complete
                        // camera/trait/death acceptance suite passes. The release
                        // validator still requires verifiedInGame, never this flag.
                        ScaleApproved = approved && ((bool?)calibration?["verifiedInGame"] == true
                            || (bool?)calibration?["previewScaleApproved"] == true),
                        Scale = (float?)calibration?["renderScale"] ?? 0f
                    };
                    form.NativeResourcePath = ((string)group["nativeResourcePath"])?.TrimEnd('/');
                    if (form.NativeResourcePath != null)
                    {
                        if (version != 2 || !form.AnchorApproved || form.NativeResourcePath.Length == 0)
                            throw new InvalidDataException("Unreviewed native resource group: " + path);
                        ResolveNativeFile(form.NativeResourcePath + "/main/idle_0");
                    }
                    if (form.Width <= 0 || form.Height <= 0 || !Finite(form.AnchorX) || !Finite(form.AnchorY)
                        || form.AnchorX < 0 || form.AnchorX > form.Width || form.AnchorY < 0 || form.AnchorY > form.Height
                        || !Finite(form.PixelsPerUnit) || form.PixelsPerUnit < 0 || !Finite(form.Scale) || form.Scale < 0)
                        throw new InvalidDataException("Invalid actor profile: " + path);
                    if (version == 2)
                    {
                        JObject actions = group["actions"] as JObject;
                        foreach (string action in ActionNames)
                        {
                            JObject item = actions?[action] as JObject;
                            if (item?["visualFallback"] != null)
                            {
                                // User waived new combat art. Reuse an already reviewed
                                // idle pose, with a single-shot visual clock; no PNG copy.
                                double milliseconds = (double?)item["durationMs"] ?? 0;
                                if ((action != "attack" && action != "hit")
                                    || (string)item["visualFallback"] != "idle"
                                    || (bool?)item["loop"] != false
                                    || item["frames"] is not JArray fallbackFrames || fallbackFrames.Count != 0
                                    || double.IsNaN(milliseconds) || double.IsInfinity(milliseconds) || milliseconds <= 0
                                    || !form.Actions.TryGetValue("idle", out var idle) || idle.Names.Length == 0)
                                    throw new InvalidDataException("Invalid waived combat fallback: " + path + "/" + action);
                                form.Actions.Add(action, new MclslAnimationClipData(
                                    new[] { idle.Names[0] }, new[] { milliseconds / 1000d }, false));
                                continue;
                            }
                            if (item?["frames"] is not JArray frames || frames.Count == 0)
                                throw new InvalidDataException("Missing action: " + path + "/" + action);
                            string[] names = new string[frames.Count];
                            double[] durations = new double[frames.Count];
                            var unique = new HashSet<string>(StringComparer.Ordinal);
                            for (int i = 0; i < frames.Count; i++)
                            {
                                names[i] = (string)frames[i]["name"];
                                durations[i] = (double)frames[i]["durationMs"] / 1000d;
                                if (names[i] == null || !names[i].StartsWith(action + "_", StringComparison.Ordinal)
                                    || !unique.Add(names[i])) throw new InvalidDataException("Invalid/duplicate frame name");
                                if (frames[i]["canvas"] != null || frames[i]["anchor"] != null
                                    || frames[i]["pixelsPerUnit"] != null)
                                {
                                    JArray frameCanvas = frames[i]["canvas"] as JArray;
                                    JArray frameAnchor = frames[i]["anchor"] as JArray;
                                    if (frameCanvas?.Count != 2 || frameAnchor?.Count != 2
                                        || frames[i]["pixelsPerUnit"] == null)
                                        throw new InvalidDataException("Incomplete frame geometry: " + names[i]);
                                    FrameProfile frameProfile = new()
                                    {
                                        Width = (int)frameCanvas[0], Height = (int)frameCanvas[1],
                                        AnchorX = (float)frameAnchor[0], AnchorY = (float)frameAnchor[1],
                                        PixelsPerUnit = (float)frames[i]["pixelsPerUnit"],
                                        AnchorApproved = form.AnchorApproved && ((bool?)frames[i]["anchorReviewed"] != false
                                            || integrated && (bool?)frames[i]["geometryConfigured"] == true)
                                    };
                                    if (frameProfile.Width <= 0 || frameProfile.Height <= 0
                                        || !Finite(frameProfile.AnchorX) || !Finite(frameProfile.AnchorY)
                                        || frameProfile.AnchorX < 0 || frameProfile.AnchorX > frameProfile.Width
                                        || frameProfile.AnchorY < 0 || frameProfile.AnchorY > frameProfile.Height
                                        || !Finite(frameProfile.PixelsPerUnit) || frameProfile.PixelsPerUnit <= 0)
                                        throw new InvalidDataException("Invalid frame geometry: " + names[i]);
                                    form.FrameProfiles.Add(names[i], frameProfile);
                                }
                                if (form.NativeResourcePath != null)
                                {
                                    if (!form.FrameProfiles.TryGetValue(names[i], out FrameProfile nativeFrame)
                                        || !nativeFrame.AnchorApproved)
                                        throw new InvalidDataException("Native frame requires reviewed per-frame geometry: " + names[i]);
                                    ResolveNativeFile(form.NativeResourcePath + "/main/" + names[i]);
                                }
                            }
                            bool loop = (bool?)item["loop"] ?? (action == "idle" || action == "walk");
                            if (loop != (action == "idle" || action == "walk"))
                                throw new InvalidDataException("Invalid action loop: " + action);
                            form.Actions.Add(action, new MclslAnimationClipData(names, durations, loop));
                        }
                    }
                    if (Forms.ContainsKey(path)) throw new InvalidDataException("Duplicate actor profile: " + path);
                    Forms.Add(path, form);
                }
                catch (Exception ex) { MclslDiagnostics.Error("actor-manifest-group", ex.Message); }
            }
        }
        catch (Exception ex) { MclslDiagnostics.Error("actor-manifest", ex.Message); }
    }

    // A manifestless 0.5.4 installation remains usable; scan once per cached form/action.
    private static readonly Dictionary<string, MclslAnimationClipData> Legacy = new(StringComparer.Ordinal);
    private static MclslAnimationClipData DiscoverLegacy(string formPath, string action)
    {
        if (formPath == null) return null;
        string key = formPath.TrimEnd('/') + "/main/" + action;
        if (Legacy.TryGetValue(key, out MclslAnimationClipData cached)) return cached;
        MclslAnimationClipData clip = null;
        try
        {
            string directory = Path.GetDirectoryName(ResolveFile(key + "_0"));
            var ordered = new SortedDictionary<int, string>();
            if (Directory.Exists(directory))
                foreach (string file in Directory.GetFiles(directory, action + "_*.png"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (int.TryParse(name.Substring(action.Length + 1), NumberStyles.None,
                        CultureInfo.InvariantCulture, out int index) && index >= 0) ordered[index] = name;
                }
            if (ordered.Count > 0)
            {
                string[] names = new string[ordered.Count];
                ordered.Values.CopyTo(names, 0);
                double seconds = action == "idle" ? 14d / 60 : action == "walk" ? 10d / 60
                    : action == "attack" ? 6d / 60 : action == "hit" ? 5d / 60 : 7d / 60;
                double[] durations = new double[names.Length];
                for (int i = 0; i < durations.Length; i++) durations[i] = seconds;
                clip = new MclslAnimationClipData(names, durations, action == "idle" || action == "walk");
            }
        }
        catch (Exception ex) { MclslDiagnostics.Error("actor-legacy:" + key, ex.Message); }
        Legacy[key] = clip;
        return clip;
    }

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
