using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// 独立于 Harmony 模拟钩子的运行时驱动。MapBox.updateSimulation 补丁仍保留，
/// 但即使目标方法签名变化或补丁被其他模组覆盖，年度队列也会每个渲染帧获得
/// 一次有界消费机会。MclslRuntime.Tick 自身按 Unity 帧去重，因此双入口不会重复结算。
/// </summary>
internal sealed class MclslRuntimeDriver : MonoBehaviour
{
    private const string DriverObjectName = "MclslRuntimeDriver";
    private static MclslRuntimeDriver _instance;

    internal static void Ensure()
    {
        if (_instance != null) return;

        GameObject host = GameObject.Find(DriverObjectName);
        if (host == null)
        {
            host = new GameObject(DriverObjectName);
            host.hideFlags = HideFlags.HideAndDontSave;
        }

        _instance = host.GetComponent<MclslRuntimeDriver>();
        if (_instance == null) _instance = host.AddComponent<MclslRuntimeDriver>();
        Object.DontDestroyOnLoad(host);
    }

    private void Update() => MclslRuntime.Tick();

    private void OnDestroy()
    {
        if (ReferenceEquals(_instance, this)) _instance = null;
    }
}
