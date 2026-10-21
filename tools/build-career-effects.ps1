param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$icons = Join-Path $root 'GameResources/ui/CareerSkills'
$output = Join-Path $root 'GameResources/effects/Careers'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$careers = @('sword', 'martial', 'elemental', 'sigil', 'array', 'spirit')
$colors = @(
    [System.Drawing.Color]::Cyan, [System.Drawing.Color]::Orange,
    [System.Drawing.Color]::DeepSkyBlue, [System.Drawing.Color]::Gold,
    [System.Drawing.Color]::MediumTurquoise, [System.Drawing.Color]::LightGreen
)

for ($career = 0; $career -lt 6; $career++) {
    for ($tier = 1; $tier -le 5; $tier++) {
        $id = 'CP_' + $careers[$career] + '_' + $tier + 'A'
        $icon = [System.Drawing.Bitmap]::new((Join-Path $icons ($id + '.png')))
        $atlas = [System.Drawing.Bitmap]::new(256, 384, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($atlas)
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
        for ($frame = 0; $frame -lt 24; $frame++) {
            $phase = if ($frame -lt 8) { 0 } elseif ($frame -lt 16) { 1 } elseif ($frame -lt 20) { 2 } else { 3 }
            $localFrame = if ($phase -eq 0) { $frame } elseif ($phase -eq 1) { $frame - 8 }
                elseif ($phase -eq 2) { $frame - 16 } else { $frame - 20 }
            $progress = if ($phase -lt 2) { $localFrame / 7.0 } else { $localFrame / 3.0 }
            $scale = if ($phase -eq 0) { 0.38 + 0.45 * $progress }
                elseif ($phase -eq 1) { 0.65 + 0.16 * [Math]::Sin($progress * [Math]::PI) }
                elseif ($phase -eq 2) { 0.85 + 0.40 * $progress }
                else { 0.8 - 0.5 * $progress }
            $alpha = if ($phase -eq 0) { 0.3 + 0.7 * $progress }
                elseif ($phase -eq 2 -or $phase -eq 3) { 1.0 - 0.85 * $progress } else { 1.0 }
            $angle = switch ($career) {
                0 { -25 + 45 * $progress }
                1 { 4 * [Math]::Sin($frame * 2) }
                2 { 24 * $progress }
                3 { -12 + 24 * $progress }
                4 { 45 * $progress }
                5 { 8 * [Math]::Sin($frame) }
            }
            $column = $frame % 4
            $row = [int][Math]::Floor($frame / 4)
            $centerX = $column * 64 + 32
            $centerY = $row * 64 + 32
            $travel = if ($phase -eq 1 -and $career -in @(0, 2, 3, 5)) { 14 * ($progress - 0.5) } else { 0 }
            $graphics.TranslateTransform([single]($centerX + $travel), [single]$centerY)
            $graphics.RotateTransform([single]$angle)
            $size = [int][Math]::Round(58 * $scale)
            $matrix = [System.Drawing.Imaging.ColorMatrix]::new()
            $matrix.Matrix33 = [single]$alpha
            $attributes = [System.Drawing.Imaging.ImageAttributes]::new()
            $attributes.SetColorMatrix($matrix)
            $graphics.DrawImage($icon, [System.Drawing.Rectangle]::new(-[int]($size/2), -[int]($size/2), $size, $size),
                0, 0, 64, 64, [System.Drawing.GraphicsUnit]::Pixel, $attributes)
            $attributes.Dispose()
            if ($phase -eq 2 -or $career -eq 4) {
                $ringAlpha = if ($phase -eq 2) { [int](180 * (1 - $progress)) } else { 95 }
                $ring = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb($ringAlpha, $colors[$career]), 2 + $tier / 3.0)
                $radius = if ($phase -eq 2) { 15 + 14 * $progress } else { 15 + 3 * $progress }
                $graphics.DrawEllipse($ring, [single](-$radius), [single](-$radius), [single](2*$radius), [single](2*$radius))
                $ring.Dispose()
            }
            $graphics.ResetTransform()
        }
        $graphics.Dispose()
        $atlas.Save((Join-Path $output ($id + '.png')), [System.Drawing.Imaging.ImageFormat]::Png)
        $atlas.Dispose()
        $icon.Dispose()
    }
}
