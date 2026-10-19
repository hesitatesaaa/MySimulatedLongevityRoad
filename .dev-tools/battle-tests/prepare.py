"""Test the actual terrain implementation without loading the whole Unity spell renderer."""
from pathlib import Path
root=Path(__file__).resolve().parents[2]
source=(root/"code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellSystem.cs").read_text(encoding="utf-8")
terrain=source.split("internal static class MclslSpellTerrain\n",1)[1].split("internal sealed class MclslSpellEffect",1)[0]
out=Path(__file__).parent/"obj/GeneratedTerrain.cs"
out.parent.mkdir(exist_ok=True)
out.write_text("// Extracted verbatim from production source; do not edit.\nusing System;\nusing System.Linq;\nusing System.Collections.Generic;\nusing UnityEngine;\nusing MySimulatedLongevityRoad.Core;\nusing MySimulatedLongevityRoad.Systems.Combat;\nnamespace MySimulatedLongevityRoad.Systems;\ninternal static class MclslSpellTerrain\n"+terrain,encoding="utf-8")
print("Prepared production terrain source for offline regression tests.")

def block(signature):
    start=source.index(signature)
    opening=source.index("{",start)
    depth=1
    end=opening+1
    while depth:
        depth+=(source[end]=="{")-(source[end]=="}")
        end+=1
    return source[start:end]

payment="\n".join(block(signature) for signature in (
    "private sealed class RuntimeState", "private static RuntimeState GetRuntime(Actor actor)",
    "private static void CommitCast(", "internal static string[] ReserveElementalChain("))
(out.parent/"GeneratedPayment.cs").write_text(
    "// Extracted verbatim from production source; do not edit.\nusing System;\nusing System.Collections.Generic;\nusing System.Runtime.CompilerServices;\nusing UnityEngine;\nusing MySimulatedLongevityRoad.Core;\nusing MySimulatedLongevityRoad.Data;\nusing MySimulatedLongevityRoad.Traits;\nusing MySimulatedLongevityRoad.Systems.Combat;\nnamespace MySimulatedLongevityRoad.Systems;\ninternal static partial class MclslSpellSystem\n{\n"+payment+"\n}\n",encoding="utf-8")
