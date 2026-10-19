using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Reflection;
using System.Reflection.Emit;

using var file = File.OpenRead(args[0]);
using var pe = new PEReader(file);
var reader = pe.GetMetadataReader();
if (args.Contains("--types"))
{
    foreach (var h in reader.TypeDefinitions) Console.WriteLine(reader.GetString(reader.GetTypeDefinition(h).Name));
    return;
}
var wanted = new HashSet<string>(args.Skip(1));
var ilTargets = args.Skip(1).Where(a => a.StartsWith("--il=")).Select(a => a[5..]).ToHashSet();
var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
    .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
    .ToDictionary(o => unchecked((ushort)o.Value));
string TokenName(int token)
{
    var handle = MetadataTokens.Handle(token);
    return handle.Kind switch
    {
        HandleKind.MethodDefinition => reader.GetString(reader.GetTypeDefinition(reader.GetMethodDefinition((MethodDefinitionHandle)handle).GetDeclaringType()).Name) + "::" + reader.GetString(reader.GetMethodDefinition((MethodDefinitionHandle)handle).Name),
        HandleKind.FieldDefinition => reader.GetString(reader.GetTypeDefinition(reader.GetFieldDefinition((FieldDefinitionHandle)handle).GetDeclaringType()).Name) + "::" + reader.GetString(reader.GetFieldDefinition((FieldDefinitionHandle)handle).Name),
        HandleKind.MemberReference => ParentName(reader.GetMemberReference((MemberReferenceHandle)handle).Parent) + "::" + reader.GetString(reader.GetMemberReference((MemberReferenceHandle)handle).Name),
        HandleKind.TypeDefinition => reader.GetString(reader.GetTypeDefinition((TypeDefinitionHandle)handle).Name),
        HandleKind.TypeReference => reader.GetString(reader.GetTypeReference((TypeReferenceHandle)handle).Name),
        HandleKind.UserString => "string:" + reader.GetUserString((UserStringHandle)handle),
        _ => handle.Kind + ":0x" + token.ToString("x8")
    };
}
string ParentName(EntityHandle handle) => handle.Kind switch
{
    HandleKind.TypeDefinition => reader.GetString(reader.GetTypeDefinition((TypeDefinitionHandle)handle).Name),
    HandleKind.TypeReference => reader.GetString(reader.GetTypeReference((TypeReferenceHandle)handle).Name),
    _ => handle.Kind.ToString()
};
void PrintIL(MethodDefinition method)
{
    if (method.RelativeVirtualAddress == 0) return;
    var bytes = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!;
    int position = 0;
    int ReadInt() { int v = BitConverter.ToInt32(bytes, position); position += 4; return v; }
    while (position < bytes.Length)
    {
        int offset = position;
        ushort key = bytes[position++];
        if (key == 0xfe) key = (ushort)(0xfe00 | bytes[position++]);
        var op = opcodes[key];
        string operand = "";
        switch (op.OperandType)
        {
            case OperandType.InlineNone: break;
            case OperandType.ShortInlineI: operand = ((sbyte)bytes[position++]).ToString(); break;
            case OperandType.ShortInlineVar: operand = bytes[position++].ToString(); break;
            case OperandType.InlineVar: operand = BitConverter.ToUInt16(bytes, position).ToString(); position += 2; break;
            case OperandType.InlineI: operand = ReadInt().ToString(); break;
            case OperandType.InlineI8: operand = BitConverter.ToInt64(bytes, position).ToString(); position += 8; break;
            case OperandType.ShortInlineR: operand = BitConverter.ToSingle(bytes, position).ToString(); position += 4; break;
            case OperandType.InlineR: operand = BitConverter.ToDouble(bytes, position).ToString(); position += 8; break;
            case OperandType.ShortInlineBrTarget: int delta = (sbyte)bytes[position++]; operand = "IL_" + (position + delta).ToString("x4"); break;
            case OperandType.InlineBrTarget: int jump = ReadInt(); operand = "IL_" + (position + jump).ToString("x4"); break;
            case OperandType.InlineSwitch:
                int count = ReadInt(), baseOffset = position + count * 4;
                operand = string.Join(",", Enumerable.Range(0, count).Select(_ => "IL_" + (baseOffset + ReadInt()).ToString("x4"))); break;
            default: operand = TokenName(ReadInt()); break;
        }
        Console.WriteLine($"    IL_{offset:x4} {op.Name} {operand}");
    }
}
foreach (var handle in reader.TypeDefinitions)
{
    var type = reader.GetTypeDefinition(handle);
    var name = reader.GetString(type.Name);
    bool showFields = wanted.Contains(name);
    if (!showFields && !ilTargets.Any(t => t.StartsWith(name + "::"))) continue;
    Console.WriteLine("TYPE " + name);
    foreach (var fieldHandle in showFields ? (IEnumerable<FieldDefinitionHandle>)type.GetFields() : Array.Empty<FieldDefinitionHandle>())
    {
        var field = reader.GetFieldDefinition(fieldHandle);
        Console.WriteLine("  FIELD " + reader.GetString(field.Name) + " : " + field.DecodeSignature(new Types(), null));
    }
    foreach (var methodHandle in type.GetMethods())
    {
        var method = reader.GetMethodDefinition(methodHandle);
        bool showIL = ilTargets.Contains(name + "::" + reader.GetString(method.Name));
        if (!showFields && !showIL) continue;
        var signature = method.DecodeSignature(new Types(), null);
        Console.WriteLine("  METHOD " + reader.GetString(method.Name) + "(" + string.Join(", ", signature.ParameterTypes) + ") -> " + signature.ReturnType);
        if (showIL) PrintIL(method);
    }
}

sealed class Types : ISignatureTypeProvider<string, object>
{
    public string GetArrayType(string e, ArrayShape s) => e + "[]";
    public string GetByReferenceType(string e) => e + "&";
    public string GetFunctionPointerType(MethodSignature<string> s) => "function";
    public string GetGenericInstantiation(string g, System.Collections.Immutable.ImmutableArray<string> a) => g + "<" + string.Join(",", a) + ">";
    public string GetGenericMethodParameter(object c, int i) => "!!" + i;
    public string GetGenericTypeParameter(object c, int i) => "!" + i;
    public string GetModifiedType(string m, string u, bool r) => u;
    public string GetPinnedType(string e) => e;
    public string GetPointerType(string e) => e + "*";
    public string GetPrimitiveType(PrimitiveTypeCode c) => c.ToString();
    public string GetSZArrayType(string e) => e + "[]";
    public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte k) => r.GetString(r.GetTypeDefinition(h).Name);
    public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte k) => r.GetString(r.GetTypeReference(h).Name);
    public string GetTypeFromSpecification(MetadataReader r, object c, TypeSpecificationHandle h, byte k) => r.GetTypeSpecification(h).DecodeSignature(this, c);
}
