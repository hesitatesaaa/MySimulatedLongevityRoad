using System;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 打开/切换原版人物窗口的统一入口：先结束窗口动画再打开，并在重新显示后重新绑定关系栏回调。
/// </summary>
internal static class MclslActorWindowOpener
{
    internal static void Open(Actor actor)
    {
        if (actor?.data == null) return;
        try
        {
            // 结束窗口动画，否则原版 showWindow 会静默早退，只改选择状态。
            ScrollWindow.finishAnimations();
            ActionLibrary.openUnitWindow(actor);
            // 原版隐藏/重新显示会清空关系栏回调；这里按当前显示人物重新绑定。
            MclslContractInfoBar.RefreshFromOpen();
        }
        catch (Exception ex) { MclslDiagnostics.Error("actor-window-open", "打开人物窗口失败: " + ex.Message); }
    }
}