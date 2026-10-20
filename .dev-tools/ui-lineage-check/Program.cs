using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using System.Text.Json;
using HarmonyLib;

string game=Path.GetFullPath(args[0]);
string managed=Path.Combine(game,"worldbox_Data/Managed");
string mods=Path.Combine(game,"worldbox_Data/StreamingAssets/mods");
AssemblyLoadContext.Default.Resolving += (_,name) =>
{
    string? path=name.Name=="Assembly-CSharp" ? Path.Combine(mods,"NML/Assembly-CSharp-Publicized.dll") :
        new[]{managed,mods,Path.Combine(mods,"NML/Assemblies")}.Select(dir=>Path.Combine(dir,name.Name+".dll")).FirstOrDefault(File.Exists);
    return path!=null && File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
};
var native=AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(mods,"NML/Assembly-CSharp-Publicized.dll"));
var production=AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(args[1]));
var patches=production.GetType("MySimulatedLongevityRoad.Patches.MclslPatches",true)!;
var transpiler=patches.GetMethod("SelectedUnits_Scale_Transpiler",BindingFlags.NonPublic|BindingFlags.Static)!;
int assertions=0;
void Check(bool value,string reason){if(!value)throw new Exception(reason);assertions++;}
var paths=new List<object>();
List<CodeInstruction> ReadNativeIL(MethodInfo method)
{
    var opcodes=typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static)
        .Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)!).ToDictionary(op=>unchecked((ushort)op.Value));
    using var stream=new MemoryStream(method.GetMethodBody()!.GetILAsByteArray()!);
    using var reader=new BinaryReader(stream);
    var entries=new List<(int Offset,CodeInstruction Code)>();
    var branches=new List<(CodeInstruction Code,int[] Targets)>();
    while(stream.Position<stream.Length)
    {
        int offset=(int)stream.Position;
        ushort key=reader.ReadByte();if(key==0xfe)key=(ushort)(0xfe00|reader.ReadByte());
        var op=opcodes[key];object? operand=null;int[]? targets=null;
        switch(op.OperandType)
        {
            case OperandType.InlineNone: break;
            case OperandType.ShortInlineI:operand=reader.ReadSByte();break;
            case OperandType.ShortInlineVar:operand=reader.ReadByte();break;
            case OperandType.InlineVar:operand=reader.ReadUInt16();break;
            case OperandType.InlineI:operand=reader.ReadInt32();break;
            case OperandType.InlineI8:operand=reader.ReadInt64();break;
            case OperandType.ShortInlineR:operand=reader.ReadSingle();break;
            case OperandType.InlineR:operand=reader.ReadDouble();break;
            case OperandType.InlineString:operand=method.Module.ResolveString(reader.ReadInt32());break;
            case OperandType.ShortInlineBrTarget:
                int shortDelta=reader.ReadSByte();targets=new[]{(int)stream.Position+shortDelta};break;
            case OperandType.InlineBrTarget:
                int delta=reader.ReadInt32();targets=new[]{(int)stream.Position+delta};break;
            case OperandType.InlineSwitch:
                int count=reader.ReadInt32(),end=(int)stream.Position+count*4;
                targets=Enumerable.Range(0,count).Select(_=>end+reader.ReadInt32()).ToArray();break;
            default:operand=method.Module.ResolveMember(reader.ReadInt32());break;
        }
        var code=new CodeInstruction(op,operand);entries.Add((offset,code));
        if(targets!=null)branches.Add((code,targets));
    }
    var generator=new DynamicMethod("ReadNativeLabels",typeof(void),Type.EmptyTypes).GetILGenerator();
    var labels=branches.SelectMany(b=>b.Targets).Distinct().ToDictionary(offset=>offset,_=>generator.DefineLabel());
    foreach(var entry in entries)if(labels.TryGetValue(entry.Offset,out var label))entry.Code.labels.Add(label);
    foreach(var branch in branches)branch.Code.operand=branch.Code.opcode.OperandType==OperandType.InlineSwitch
        ? branch.Targets.Select(t=>labels[t]).ToArray() : labels[branch.Targets[0]];
    return entries.Select(e=>e.Code).ToList();
}
foreach(string name in new[]{"drawSelectedUnits","drawUnitsToBeSelectedBySquareTool"})
{
    var method=native.GetType("QuantumSpriteLibrary",true)!.GetMethod(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static)!;
    Check(method!=null,"Native selection method exists: "+name);
    Check(method!.GetMethodBody()!=null,"Native selection method has real IL: "+name);
    var original=ReadNativeIL(method);
    int originalLabels=original.Sum(c=>c.labels.Count);
    var result=((IEnumerable<CodeInstruction>)transpiler.Invoke(null,new object[]{original})!).ToList();
    Check(result.Count==original.Count,"Labels and instruction offsets preserved");
    var calls=result.Where(c=>c.opcode==OpCodes.Call && c.operand is MethodInfo mi && mi.Name=="SelectionScale").ToArray();
    Check(calls.Length==1,"Replace exactly the real native selection scale load: "+name);
    Check(!result.Any(c=>c.opcode==OpCodes.Ldflda && c.operand is FieldInfo f && f.Name=="current_scale"),"No world render scale left in selection path");
    Check(result.Sum(c=>c.labels.Count)==originalLabels,"Native branch labels preserved");
    paths.Add(new{name,originalInstructions=original.Count,replacements=calls.Length});
}
bool rejected=false;
try { transpiler.Invoke(null,new object[]{Array.Empty<CodeInstruction>()}); }
catch(TargetInvocationException e) when(e.InnerException is InvalidOperationException){rejected=true;}
Check(rejected,"Changed native IL must fail explicitly, never silently pass");
foreach(var target in new[]{("UnitAvatarLoader","loadItemSprites"),("ActorAsset","getSpriteIcon"),("Subspecies","getUnitSpriteForBanner")})
    Check(native.GetType(target.Item1,true)!.GetMethod(target.Item2,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance,null,Type.EmptyTypes,null)!=null,"Real native API: "+target);
Check(native.GetType("Subspecies",true)!.GetMethod("generateName",new[]{native.GetType("ActorAsset")!,native.GetType("WorldTile")!})!=null,"Actual species creation naming overload");
var presentation=production.GetType("MySimulatedLongevityRoad.Systems.MclslActorPresentation",true)!;
var canonical=presentation.GetMethod("CanonicalSpeciesId",BindingFlags.Static|BindingFlags.NonPublic)!;
using var manifest=JsonDocument.Parse(File.ReadAllText(args[2]));
int mappings=0;
foreach(var group in manifest.RootElement.GetProperty("groups").EnumerateArray())
{
    string id=group.GetProperty("actorId").GetString()!;
    string expected=id.EndsWith("_yuan",StringComparison.Ordinal)?id[..^5]:id;
    Check((string)canonical.Invoke(null,new object[]{id})! == expected,"Real catalog lineage mapping: "+id);mappings++;
}
foreach(string id in new[]{"human","orc","sheep","mclsl_named_zhangfan","mclsl_world_soul_jin","mclsl_world_soul_avatar"})
    Check((string)canonical.Invoke(null,new object[]{id})! == id,"Preserve distinct native/named/soul identity: "+id);
Console.WriteLine(JsonSerializer.Serialize(new{assertions,paths,mappings,actualProductionTranspiler=true,realGameIL=true,mockOrStubUsed=false,unityTested=false}));
