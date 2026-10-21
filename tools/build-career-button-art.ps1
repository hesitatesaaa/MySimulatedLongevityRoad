param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$output = Join-Path (Split-Path -Parent $PSScriptRoot) 'GameResources/ui/XuanhuangSkin'
$captions = @{ title_career_skills = '职业技能'; title_career_archive = '修士档案' }
foreach ($name in $captions.Keys) {
    $image = [System.Drawing.Bitmap]::new(320, 80, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($image)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $font = [System.Drawing.FontFamily]::new('华文行楷')
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $format = [System.Drawing.StringFormat]::new()
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    $path.AddString($captions[$name], $font, 0, 55,
        [System.Drawing.RectangleF]::new(6, 4, 308, 72), $format)
    $glow = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(90, 0, 177, 156), 7)
    $glow.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $graphics.DrawPath($glow, $path)
    $outline = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 20, 59, 48), 4)
    $outline.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $graphics.DrawPath($outline, $path)
    $gold = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.RectangleF]::new(0, 8, 320, 64),
        [System.Drawing.Color]::FromArgb(255, 255, 242, 167),
        [System.Drawing.Color]::FromArgb(255, 211, 146, 55), 90)
    $graphics.FillPath($gold, $path)
    $image.Save((Join-Path $output ($name + '.png')), [System.Drawing.Imaging.ImageFormat]::Png)
    $gold.Dispose(); $outline.Dispose(); $glow.Dispose(); $path.Dispose()
    $format.Dispose(); $font.Dispose(); $graphics.Dispose(); $image.Dispose()
}
