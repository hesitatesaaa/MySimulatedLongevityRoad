namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 右侧修士档案的排版片段。档案正文与「斗法传承」共用同一组标记，
/// 这样无论内容由哪个系统生成，栏目、字段名与分隔符的样式都一致。
/// 字体本身由承载的 Text 统一设置（见 MclslUiTheme.ActorPanelText*）。
/// </summary>
internal static class MclslActorArchiveText
{
    internal static string Title(string title) => "<color=#A6D8D1><b>◇ " + title + "</b></color>\n";
    internal static string Section(string title) => "\n<color=#B9EEE4><b>┄ " + title + " ┄</b></color>\n";
    internal static string Line(string label, string value) =>
        "  <color=#D8CDAA>" + label + "</color> <color=#596A70>·</color> "
        + (string.IsNullOrWhiteSpace(value) ? "无" : value) + "\n";
    internal static string Highlight(string value, string color) =>
        string.IsNullOrWhiteSpace(value) ? "无" : "<color=" + color + ">" + value + "</color>";
    internal static string Blank(string value) => string.IsNullOrWhiteSpace(value) ? "无" : value;
    internal static string ReplaceTags(string value) =>
        string.IsNullOrWhiteSpace(value) ? "无" : value.Replace(",", "、");
}
