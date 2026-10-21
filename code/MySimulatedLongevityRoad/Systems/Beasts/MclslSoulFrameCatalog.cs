using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using MySimulatedLongevityRoad.Core;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslSoulFrameCatalog
{
    private static Dictionary<string, Dictionary<string, string[]>> _groups;
    internal static Sprite[] Load(string folder, string action)
    {
        EnsureLoaded();
        if (!_groups.TryGetValue(folder ?? string.Empty, out var actions)
            || !actions.TryGetValue(action, out var paths)) return Array.Empty<Sprite>();
        var sprites = new Sprite[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            sprites[i] = SpriteTextureLoader.getSprite(paths[i]);
            if (sprites[i] == null) throw new FileNotFoundException("Soul frame was not imported: " + paths[i]);
            // Soul sprites are authored whole-body art, not phenotype inputs.
            MclslSpriteOwnership.Register(sprites[i]);
        }
        return sprites;
    }

    private static void EnsureLoaded()
    {
        if (_groups != null) return;
        string file = Path.Combine(MySimulatedLongevityRoad.MclslMod.I.GetDeclaration().FolderPath,
            "resources", "world-soul-frames.manifest.json");
        JObject document = JObject.Parse(File.ReadAllText(file));
        if ((int?)document["schemaVersion"] != 1) throw new InvalidDataException("Unsupported soul frame catalog");
        var groups = document["groups"]?.ToObject<Dictionary<string, Dictionary<string, string[]>>>();
        if (groups == null) throw new InvalidDataException("Missing soul groups");
        foreach (var actions in groups.Values)
        foreach (var paths in actions.Values)
        foreach (string path in paths)
            if (string.IsNullOrEmpty(path) || !path.StartsWith("actors/Souls/", StringComparison.Ordinal)
                || path.Contains("..") || path.Contains("\\")) throw new InvalidDataException("Invalid soul frame path");
        if (document["atlas"] is JObject atlas) LoadAtlas(atlas);
        _groups = groups; // Publish only the complete validated catalog.
    }

    private static void LoadAtlas(JObject atlas)
    {
        using var timing = MclslStartupTiming.Measure("MCLS/Load/SoulAtlas");
        if (!MclslRuntimeChanges.IsMainThread) throw new InvalidOperationException("Soul atlas must initialize on the main thread");
        JArray canvas = atlas["canvas"] as JArray;
        if (canvas?.Count != 2) throw new InvalidDataException("Missing soul atlas canvas");
        int width = (int)canvas[0], height = (int)canvas[1];
        long size = (long)width * height * 4;
        if (width <= 0 || height <= 0 || width > SystemInfo.maxTextureSize || height > SystemInfo.maxTextureSize
            || size <= 0 || size > 128L * 1024 * 1024) throw new InvalidDataException("Invalid soul atlas size");
        string root = Path.GetFullPath(MclslActorAnimationCatalog.NativeResourceRoot).TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string file = Path.GetFullPath(Path.Combine(root, (string)atlas["file"] ?? string.Empty));
        if (!file.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Soul atlas escapes resource directory");
        byte[] raw = new byte[(int)size];
        using (var stream = new GZipStream(File.OpenRead(file), CompressionMode.Decompress))
        {
            int offset = 0;
            while (offset < raw.Length)
            {
                int count = stream.Read(raw, offset, raw.Length - offset);
                if (count == 0) throw new InvalidDataException("Truncated soul atlas");
                offset += count;
            }
            if (stream.ReadByte() != -1) throw new InvalidDataException("Oversized soul atlas payload");
        }
        var rectangles = atlas["rectangles"]?.ToObject<Dictionary<string, int[]>>();
        if (rectangles == null) throw new InvalidDataException("Missing soul atlas rectangles");
        var sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        Texture2D texture = null;
        try
        {
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "mclsl_soul_atlas", filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp, anisoLevel = 0
            };
            texture.LoadRawTextureData(raw);
            texture.Apply(false, true);
            foreach (var pair in rectangles)
            {
                int[] rect = pair.Value;
                if (!pair.Key.StartsWith("actors/Souls/", StringComparison.Ordinal)
                    || rect?.Length != 4 || rect[0] < 0 || rect[1] < 0 || rect[2] <= 0 || rect[3] <= 0
                    || (long)rect[0] + rect[2] > width || (long)rect[1] + rect[3] > height)
                    throw new InvalidDataException("Invalid soul atlas rectangle: " + pair.Key);
                Sprite sprite = Sprite.Create(texture, new Rect(rect[0], rect[1], rect[2], rect[3]),
                    new Vector2(.5f, .5f), 1f, 0, SpriteMeshType.FullRect);
                if (sprite == null) throw new InvalidDataException("Soul atlas sprite creation failed: " + pair.Key);
                sprite.name = Path.GetFileName(pair.Key);
                sprites.Add(pair.Key, sprite);
            }
        }
        catch
        {
            foreach (Sprite sprite in sprites.Values) UnityEngine.Object.Destroy(sprite);
            if (texture != null) UnityEngine.Object.Destroy(texture);
            throw;
        }
        foreach (var pair in sprites)
        {
            SpriteTextureLoader._cached_sprites[pair.Key] = pair.Value;
            MclslSpriteOwnership.Register(pair.Value);
        }
    }
}
