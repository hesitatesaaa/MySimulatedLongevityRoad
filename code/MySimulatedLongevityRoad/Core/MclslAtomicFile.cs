using System;
using System.IO;
using System.Text;

namespace MySimulatedLongevityRoad.Core;

internal enum MclslArchiveReadResult : byte { Missing, Primary, Backup, Invalid }

internal static class MclslAtomicFile
{
    internal static MclslArchiveReadResult Read<T>(string path, Func<string, T> parse, out T value) where T : class
    {
        value = null;
        bool found = false;
        for (int generation = 0; generation < 2; generation++)
        {
            string candidate = generation == 0 ? path : path + ".bak";
            if (!File.Exists(candidate)) continue;
            found = true;
            try
            {
                T parsed = parse(File.ReadAllText(candidate));
                if (parsed == null) continue;
                value = parsed;
                return generation == 0 ? MclslArchiveReadResult.Primary : MclslArchiveReadResult.Backup;
            }
            catch { /* Try only the previous committed generation, never a temporary file. */ }
        }
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
