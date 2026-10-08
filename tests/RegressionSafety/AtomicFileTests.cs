using MySimulatedLongevityRoad.Core;

internal static class AtomicFileTests
{
    internal static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "mclsl-atomic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "archive.json");
        try
        {
            MclslAtomicFile.Write(path, "第一代");
            MclslAtomicFile.Write(path, "第二代");
            if (File.ReadAllText(path) != "第二代" || File.ReadAllText(path + ".bak") != "第一代")
                throw new Exception("atomic replacement must retain the previous generation");
            MclslAtomicFile.Write(path, "恢复后", preserveBackup: true);
            if (File.ReadAllText(path) != "恢复后" || File.ReadAllText(path + ".bak") != "第一代")
                throw new Exception("recovery must preserve the validated backup");
            using (FileStream locked = new(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                bool failed = false;
                try { MclslAtomicFile.Write(path, "不得写入"); }
                catch (IOException) { failed = true; }
                if (!failed) throw new Exception("locked destination must reject replacement");
            }
            if (File.ReadAllText(path) != "恢复后" || Directory.GetFiles(directory, "*.tmp").Length != 0)
                throw new Exception("failed write must preserve data and clean its private temporary file");
            static string? Parse(string text) => text is "恢复后" or "第一代" ? text : null;
            if (MclslAtomicFile.Read(path, Parse, out string? primary) != MclslArchiveReadResult.Primary || primary != "恢复后")
                throw new Exception("valid primary takes precedence over backup");
            File.WriteAllText(path, "损坏");
            if (MclslAtomicFile.Read(path, Parse, out string? backup) != MclslArchiveReadResult.Backup || backup != "第一代")
                throw new Exception("invalid primary must recover validated backup");
            File.WriteAllText(path + ".bak", "也损坏");
            if (MclslAtomicFile.Read(path, Parse, out _) != MclslArchiveReadResult.Invalid
                || File.ReadAllText(path) != "损坏")
                throw new Exception("invalid generations must be reported without modifying caller files");
            if (MclslAtomicFile.Read(Path.Combine(directory, "missing"), Parse, out _) != MclslArchiveReadResult.Missing)
                throw new Exception("new archive differs from corrupt archive");
        }
        finally
        {
            // Delete only the uniquely created test directory, never caller paths.
            foreach (string file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }
    }
}
