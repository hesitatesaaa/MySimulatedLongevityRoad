using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 玄黄界面统一色板。用户可见名称使用中文；稳定的内部标识继续使用 ASCII，
/// 避免改名破坏存档、按钮绑定和其他模组兼容性。
/// </summary>
internal static class MclslUiTheme
{
    internal static int ReadableFontSize(int size) => Mathf.Max(size + 1, (size * 11 + 5) / 10);

    internal static readonly Color Frame = new(0.16f, 0.17f, 0.19f, 0.99f);
    internal static readonly Color FrameGold = new(0.68f, 0.53f, 0.24f, 0.96f);
    internal static readonly Color SurfaceWindow = new(0.035f, 0.075f, 0.078f, 0.99f);
    internal static readonly Color SurfacePanel = new(0.065f, 0.115f, 0.112f, 0.98f);
    internal static readonly Color SurfaceDeep = new(0.025f, 0.057f, 0.060f, 0.99f);
    internal static readonly Color SurfaceScroll = new(0.020f, 0.045f, 0.047f, 0.96f);
    internal static readonly Color AccentGold = new(0.94f, 0.78f, 0.35f, 1f);
    internal static readonly Color AccentBlue = new(0.57f, 0.76f, 0.88f, 1f);
    internal static readonly Color AccentJade = new(0.49f, 0.77f, 0.68f, 1f);
    internal static readonly Color TextPrimary = new(0.91f, 0.92f, 0.90f, 1f);
    internal static readonly Color TextMuted = new(0.61f, 0.63f, 0.61f, 1f);
    internal static readonly Color Danger = new(0.58f, 0.20f, 0.16f, 0.95f);

    // 角色侧栏采用偏青的半透明卷面，和地图背景拉开层次，同时保留文字可读性。
    internal static readonly Color ActorPanelSurface = new(0.12f, 0.30f, 0.29f, 0.60f);
    internal static readonly Color ActorPanelEdge = new(0.47f, 0.78f, 0.72f, 0.58f);
    internal static readonly Color ActorPanelAccent = new(0.58f, 0.90f, 0.82f, 0.86f);
    internal static readonly Color ActorPanelText = new(0.91f, 0.97f, 0.95f, 1f);
    // 角色侧栏正文的唯一字号与行距来源。修士档案与「职业技能」页的
    // 斗法传承正文都取这里，避免两处各写一套而出现字体差异。
    internal static readonly int ActorPanelTextSize = ReadableFontSize(10);
    internal const float ActorPanelTextLineSpacing = 0.95f;

    // 保留深色卷面和栏目底色；透明边框之外不再铺灰绿色底板。
    internal static readonly Color RankFrameEdge = new(0.42f, 0.61f, 0.62f, 0.88f);
    internal static readonly Color RankSurfaceWindow = new(0.20f, 0.27f, 0.23f, 1f);
    internal static readonly Color RankSurfacePanel = new(0.16f, 0.23f, 0.20f, 0.98f);
    internal static readonly Color RankSurfaceDeep = new(0.09f, 0.14f, 0.12f, 1f);
    internal static readonly Color RankSurfaceScroll = new(0.07f, 0.11f, 0.095f, 0.99f);
    internal static readonly Color RankAccentPrimary = new(0.62f, 0.76f, 0.78f, 1f);
    internal static readonly Color RankAccentSecondary = new(0.72f, 0.84f, 0.80f, 1f);
    internal static readonly Color RankTextPrimary = new(0.92f, 0.94f, 0.87f, 1f);
    internal static readonly Color RankTextEmphasis = new(0.78f, 0.88f, 0.82f, 1f);
    internal static readonly Color RankTextMuted = new(0.68f, 0.70f, 0.64f, 1f);
    internal static readonly Color RankButton = new(0.16f, 0.23f, 0.20f, 0.98f);
    internal static readonly Color RankButtonActive = new(0.29f, 0.31f, 0.25f, 0.98f);
    internal static readonly Color RankHighlight = new(0.28f, 0.48f, 0.46f, 0.94f);
    internal static readonly Color RankMedalFirst = new(1.0f, 0.82f, 0.40f, 1f);
    internal static readonly Color RankMedalSecond = new(0.79f, 0.88f, 0.96f, 1f);
    internal static readonly Color RankMedalThird = new(0.84f, 0.55f, 0.35f, 1f);
    internal static readonly Color RankRowFirst = new(0.30f, 0.23f, 0.10f, 0.98f);
    internal static readonly Color RankRowSecond = new(0.15f, 0.22f, 0.30f, 0.98f);
    internal static readonly Color RankRowThird = new(0.28f, 0.16f, 0.10f, 0.98f);
    internal static readonly Color RankRowEven = new(0.20f, 0.27f, 0.23f, 0.98f);
    internal static readonly Color RankRowOdd = new(0.15f, 0.22f, 0.18f, 0.98f);
    internal static readonly Color RankDanger = new(0.36f, 0.22f, 0.20f, 0.96f);

    // 神兽榜独立配色：玄黑暗红古金，与 frame_beast_ranking.png 匹配，
    // 不复用玄黄修士榜的绿色卷面色板。
    internal static readonly Color BeastRankSurfaceWindow = new(0.050f, 0.034f, 0.030f, 0.99f);
    internal static readonly Color BeastRankSurfacePanel = new(0.100f, 0.060f, 0.050f, 0.98f);
    internal static readonly Color BeastRankSurfaceDeep = new(0.035f, 0.022f, 0.020f, 0.99f);
    internal static readonly Color BeastRankSurfaceScroll = new(0.030f, 0.018f, 0.016f, 0.96f);
    internal static readonly Color BeastRankButton = new(0.130f, 0.078f, 0.064f, 0.98f);
    internal static readonly Color BeastRankHeader = new(0.300f, 0.110f, 0.088f, 0.98f);
    internal static readonly Color BeastRankAccentGold = new(0.870f, 0.700f, 0.340f, 1f);
    internal static readonly Color BeastRankAccentRed = new(0.700f, 0.220f, 0.180f, 1f);
    internal static readonly Color BeastRankTextPrimary = new(0.940f, 0.890f, 0.790f, 1f);
    internal static readonly Color BeastRankTextEmphasis = new(0.900f, 0.760f, 0.500f, 1f);
    internal static readonly Color BeastRankTextMuted = new(0.680f, 0.580f, 0.500f, 1f);
    internal static readonly Color BeastRankFrameEdge = new(0.450f, 0.200f, 0.150f, 0.92f);
    internal static readonly Color BeastRankRowFirst = RankRowFirst;
    internal static readonly Color BeastRankRowSecond = RankRowSecond;
    internal static readonly Color BeastRankRowThird = RankRowThird;
    internal static readonly Color BeastRankRowEven = new(0.110f, 0.065f, 0.050f, 0.98f);
    internal static readonly Color BeastRankRowOdd = new(0.075f, 0.045f, 0.038f, 0.98f);
    internal static readonly Color BeastRankMedalFirst = RankMedalFirst;
    internal static readonly Color BeastRankMedalSecond = RankMedalSecond;
    internal static readonly Color BeastRankMedalThird = RankMedalThird;
}
