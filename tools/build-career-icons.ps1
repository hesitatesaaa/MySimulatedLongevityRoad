param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root '.dev-tools/combat-art'
$careers = @('sword', 'martial', 'elemental', 'sigil', 'array', 'spirit')
$traitOutput = Join-Path $root 'GameResources/trait'
$skillOutput = Join-Path $root 'GameResources/ui/CareerSkills'
New-Item -ItemType Directory -Force -Path $traitOutput, $skillOutput | Out-Null

function Export-Cell($atlas, [int]$columns, [int]$rows, [int]$column, [int]$row, [string]$destination) {
    $cellWidth = $atlas.Width / $columns
    $cellHeight = $atlas.Height / $rows
    $sourceRect = [System.Drawing.Rectangle]::new(
        [int]($column * $cellWidth), [int]($row * $cellHeight),
        [int]$cellWidth, [int]$cellHeight)
    $icon = [System.Drawing.Bitmap]::new(64, 64, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($icon)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    $graphics.DrawImage($atlas, [System.Drawing.Rectangle]::new(0, 0, 64, 64),
        $sourceRect, [System.Drawing.GraphicsUnit]::Pixel)
    $graphics.Dispose()
    $icon.Save($destination, [System.Drawing.Imaging.ImageFormat]::Png)
    $icon.Dispose()
}

$active = [System.Drawing.Bitmap]::new((Join-Path $source 'career_active_atlas.png'))
$passive = [System.Drawing.Bitmap]::new((Join-Path $source 'career_passive_atlas.png'))
$trait = [System.Drawing.Bitmap]::new((Join-Path $source 'career_trait_atlas.png'))
for ($career = 0; $career -lt 6; $career++) {
    Export-Cell $trait 3 2 ($career % 3) ([int][Math]::Floor($career / 3)) `
        (Join-Path $traitOutput ('MclslCombatCareer_' + $careers[$career] + '.png'))
    for ($tier = 1; $tier -le 5; $tier++) {
        Export-Cell $active 6 5 $career ($tier - 1) `
            (Join-Path $skillOutput ('CP_' + $careers[$career] + '_' + $tier + 'A.png'))
        Export-Cell $passive 6 5 $career ($tier - 1) `
            (Join-Path $traitOutput ('MclslCombatPassive_' + $careers[$career] + '_' + $tier + '.png'))
    }
}
$active.Dispose()
$passive.Dispose()
$trait.Dispose()
