using System;
using System.Reflection;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// Shared reflection accessor for optional native numeric members. Five systems
/// carried private copies of this lookup (cultivation, ancient law, world soul
/// and death). The <c>site</c> argument keeps the per-file diagnostic identity;
/// <paramref name="diagnosticKey"/> and <paramref name="messagePrefix"/> let a
/// caller that already had a different log line keep it without keeping a second
/// implementation.
/// </summary>
internal static class MclslReflectionNumbers
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public
        | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

    internal static void TrySetNumber(object target, string name, float value, string site,
        string diagnosticKey = null, string messagePrefix = null)
    {
        if (target == null || string.IsNullOrWhiteSpace(name)) return;
        try
        {
            FieldInfo field = target.GetType().GetField(name, Flags);
            if (field != null)
            {
                if (field.FieldType == typeof(float)) field.SetValue(target, value);
                else if (field.FieldType == typeof(int)) field.SetValue(target, (int)value);
                return;
            }
            PropertyInfo property = target.GetType().GetProperty(name, Flags);
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(float)) property.SetValue(target, value);
                else if (property.PropertyType == typeof(int)) property.SetValue(target, (int)value);
            }
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error(diagnosticKey ?? ("reflection-set-number:" + site),
                (messagePrefix ?? ("空 catch 捕获: code/MySimulatedLongevityRoad/" + site + ": ")) + ex.Message);
        }
    }
}
