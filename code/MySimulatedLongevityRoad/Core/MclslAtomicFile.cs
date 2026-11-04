using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Core;

internal enum MclslArchiveReadResult : byte { Missing, Primary, Backup, Invalid }

internal static class MclslAtomicFile
{
    internal static MclslArchiveReadResult Read<T>(string path, Func<string, T> parse, out T value) where T : class
        => Read(path, parse, out value, out _);

    internal static MclslArchiveReadResult Read<T>(string path, Func<string, T> parse, out T value, out string failure) where T : class
    {
        value = null;
        bool found = false;
        var errors = new List<string>();
        for (int generation = 0; generation < 2; generation++)
        {
            string candidate = generation == 0 ? path : path + ".bak";
            try
            {
                string json = File.ReadAllText(candidate);
                found = true;
                T parsed = parse(json);
                if (parsed == null) throw new InvalidDataException("Archive is null");
                value = parsed;
                failure = string.Join("; ", errors);
                return generation == 0 ? MclslArchiveReadResult.Primary : MclslArchiveReadResult.Backup;
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (Exception ex)
            {
                found = true;
                errors.Add(Path.GetFileName(candidate) + ": " + ex.GetType().Name + ": " + ex.Message);
            }
        }
        failure = string.Join("; ", errors);
        return found ? MclslArchiveReadResult.Invalid : MclslArchiveReadResult.Missing;
    }

    // Temporary and destination files share a directory so replacement stays on
    // one volume. A failed replacement must leave the previous generation intact.
    internal static void Write(string path, string contents, bool preserveBackup = false)
    {
        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        string temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (FileStream stream = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(contents ?? string.Empty);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(fullPath)) File.Replace(temporary, fullPath, preserveBackup ? null : fullPath + ".bak");
            else File.Move(temporary, fullPath);
        }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch { /* A stale private temporary file cannot invalidate a committed archive. */ }
        }
    }
}
