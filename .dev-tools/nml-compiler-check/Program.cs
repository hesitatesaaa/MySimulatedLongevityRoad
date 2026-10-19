using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

if (args.Length != 3) throw new ArgumentException("Expected source-selection.json, game directory, output DLL");
using var selection = JsonDocument.Parse(File.ReadAllText(args[0]));
string[] sources = selection.RootElement.GetProperty("sources").EnumerateArray().Select(value => value.GetString()!).ToArray();
string data = Path.Combine(args[1], "worldbox_Data");
string mods = Path.Combine(data, "StreamingAssets/mods");
var references = Directory.GetFiles(Path.Combine(data, "Managed"), "*.dll")
    .Where(path => Path.GetFileName(path) != "Assembly-CSharp.dll")
    .Concat(new[] { Path.Combine(mods, "NML/Assembly-CSharp-Publicized.dll"), Path.Combine(mods, "NeoModLoader.dll"),
        Path.Combine(mods, "NML/Assemblies/0Harmony.dll") })
    .Select(path => MetadataReference.CreateFromFile(Path.GetFullPath(path)));
var parse = new CSharpParseOptions(LanguageVersion.Latest);
var trees = sources.Select(path => CSharpSyntaxTree.ParseText(SourceText.From(File.ReadAllText(path), Encoding.UTF8), parse, path));
var compilation = CSharpCompilation.Create("MySimulatedLongevityRoad.NmlCompilerCheck", trees, references,
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, allowUnsafe: true)
        .WithNullableContextOptions(NullableContextOptions.Annotations));
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[2]))!);
using var output = File.Create(args[2]);
var result = compilation.Emit(output);
var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
var report = new { success = result.Success, sourceCount = sources.Length,
    compiler = typeof(CSharpCompilation).Assembly.FullName, errors = errors.Take(15).Select(d => d.ToString()).ToArray(),
    errorCount = errors.Length, actualUnityStartupTested = false };
string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(args[2] + ".json", json);
Console.WriteLine(json);
if (!result.Success) Environment.ExitCode = 1;
