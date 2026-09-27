param(
    [Parameter(Mandatory)][string]$ArtifactSheet1,
    [Parameter(Mandatory)][string]$ArtifactSheet2,
    [Parameter(Mandatory)][string]$ArtifactSheet3,
    [Parameter(Mandatory)][string]$MaterialSheet,
    [Parameter(Mandatory)][string]$ConsumableSheet,
    [Parameter(Mandatory)][string]$SpellSheet,
    [Parameter(Mandatory)][string]$ExtraSheet
)

Add-Type -AssemblyName System.Drawing
$resourceRoot = Join-Path $PSScriptRoot '..\GameResources\ui'

function Export-Atlas {
    param([string]$Sheet, [int]$Columns, [int]$Rows, [string[]]$Targets)
    $source = [System.Drawing.Bitmap]::new($Sheet)
    try {
        $cellWidth = [int][Math]::Floor($source.Width / $Columns)
        $cellHeight = [int][Math]::Floor($source.Height / $Rows)
        for ($i = 0; $i -lt $Targets.Length; $i++) {
            $target = $Targets[$i]
            if ([string]::IsNullOrWhiteSpace($target)) { continue }
            $col = $i % $Columns
            $row = [int][Math]::Floor($i / $Columns)
            $x0 = $col * $cellWidth
            $y0 = $row * $cellHeight
            $left = $x0 + $cellWidth
            $right = $x0
            $top = $y0 + $cellHeight
            $bottom = $y0
            for ($y = $y0; $y -lt [Math]::Min($source.Height, $y0 + $cellHeight); $y += 2) {
                for ($x = $x0; $x -lt [Math]::Min($source.Width, $x0 + $cellWidth); $x += 2) {
                    if ($source.GetPixel($x, $y).A -lt 40) { continue }
                    $left = [Math]::Min($left, $x)
                    $right = [Math]::Max($right, $x)
                    $top = [Math]::Min($top, $y)
                    $bottom = [Math]::Max($bottom, $y)
                }
            }
            if ($right -le $left -or $bottom -le $top) { throw "Empty icon cell $i in $Sheet" }
            $width = $right - $left + 3
            $height = $bottom - $top + 3
            $scale = [Math]::Min(60.0 / $width, 60.0 / $height)
            $drawWidth = [Math]::Max(1, [int][Math]::Round($width * $scale))
            $drawHeight = [Math]::Max(1, [int][Math]::Round($height * $scale))
            $output = [System.Drawing.Bitmap]::new(64, 64)
            try {
                $graphics = [System.Drawing.Graphics]::FromImage($output)
                try {
                    $graphics.Clear([System.Drawing.Color]::Transparent)
                    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                    $destination = [System.Drawing.Rectangle]::new([int][Math]::Floor((64 - $drawWidth) / 2), [int][Math]::Floor((64 - $drawHeight) / 2), $drawWidth, $drawHeight)
                    $crop = [System.Drawing.Rectangle]::new($left, $top, $width, $height)
                    $graphics.DrawImage($source, $destination, $crop, [System.Drawing.GraphicsUnit]::Pixel)
                }
                finally { $graphics.Dispose() }
                $path = Join-Path $resourceRoot ($target + '.png')
                New-Item -ItemType Directory -Path (Split-Path $path -Parent) -Force | Out-Null
                $output.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
            }
            finally { $output.Dispose() }
        }
    }
    finally { $source.Dispose() }
}

$artifactTargets = 32..79 | ForEach-Object { 'Items/B{0:D3}' -f $_ }
Export-Atlas $ArtifactSheet1 4 4 ($artifactTargets[0..15])
Export-Atlas $ArtifactSheet2 4 4 ($artifactTargets[16..31])
Export-Atlas $ArtifactSheet3 4 4 ($artifactTargets[32..47])
$materialTargets = @((10..19 | ForEach-Object { "Items/A$_" })) + @((10..19 | ForEach-Object { 'Items/F{0:D3}' -f $_ }))
Export-Atlas $MaterialSheet 5 4 $materialTargets
$consumableTargets = @((13..18 | ForEach-Object { 'Items/D{0:D3}' -f $_ })) + @((20..25 | ForEach-Object { 'Items/F{0:D3}' -f $_ }))
Export-Atlas $ConsumableSheet 6 2 $consumableTargets
$spellTargets = 1..12 | ForEach-Object { 'Spells/S{0:D3}' -f $_ }
Export-Atlas $SpellSheet 4 3 $spellTargets
Export-Atlas $ExtraSheet 4 2 @('Items/M01','Items/M02','Items/M03','Items/M04','Items/B080','Items/B081','Icons/ArtifactRain','')
