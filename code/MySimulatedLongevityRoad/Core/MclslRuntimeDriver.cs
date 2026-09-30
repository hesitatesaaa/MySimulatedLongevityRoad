using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// 唯一的渲染帧调度入口。年度队列每帧获得一次有界消费机会，
/// 原生模拟补丁只发布生命周期变化，不再另行执行模组 Tick。
/// </summary>
internal sealed partial class MclslRuntimeDriver : MonoBehaviour
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
        UnityEngine.Object.DontDestroyOnLoad(host);
    }

    private void Update() => MclslRuntime.Tick();

    private void OnDestroy()
    {
        if (ReferenceEquals(_instance, this)) _instance = null;
    }
}
