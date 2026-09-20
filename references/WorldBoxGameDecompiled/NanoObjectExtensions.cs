using System.Runtime.CompilerServices;
using JetBrains.Annotations;

public static class NanoObjectExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[ContractAnnotation("null => true")]
	public static bool isRekt(this NanoObject pObject)
	{
		if (pObject != null)
		{
			return !pObject.isAlive();
		}
		return true;
	}
}
