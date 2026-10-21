param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root '.dev-tools/combat-art'
$output = Join-Path $root 'GameResources/actors/Xuanhuang'
New-Item -ItemType Directory -Force -Path $output | Out-Null

$sources = @{
    male_1 = @('xuan_male_1.png', 'xuan_male_1_front.png', 'xuan_male_1_back.png')
    female_1 = @('xuan_female_1.png', 'xuan_female_1_front.png', 'xuan_female_1_back.png')
}
$manifest = [System.Collections.Generic.List[object]]::new()
$frameSize = 28

foreach ($skin in $sources.Keys) {
    $directions = @('', '_front', '_back')
    for ($direction = 0; $direction -lt 3; $direction++) {
        $sourceFile = Join-Path $source $sources[$skin][$direction]
        $input = [System.Drawing.Bitmap]::new($sourceFile)
        $atlas = [System.Drawing.Bitmap]::new($frameSize * 4, $frameSize * 2, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($atlas)
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $frames = [System.Collections.Generic.List[object]]::new()
        for ($row = 0; $row -lt 2; $row++) {
            for ($column = 0; $column -lt 4; $column++) {
                $left = [int][Math]::Round($column * $input.Width / 4)
                $right = [int][Math]::Round(($column + 1) * $input.Width / 4)
                $top = [int][Math]::Round($row * $input.Height / 2)
                $bottom = [int][Math]::Round(($row + 1) * $input.Height / 2)
                $sourceRect = [System.Drawing.Rectangle]::new($left, $top, $right - $left, $bottom - $top)
                $targetRect = [System.Drawing.Rectangle]::new($column * $frameSize, $row * $frameSize, $frameSize, $frameSize)
                $graphics.DrawImage($input, $targetRect, $sourceRect, [System.Drawing.GraphicsUnit]::Pixel)
                $name = if ($row -eq 0) { "walk_$column" } else { "swim_$column" }
                $frames.Add(@{
                    Name = $name; X = $column * $frameSize; Y = (1 - $row) * $frameSize
                    Width = $frameSize; Height = $frameSize; PivotX = $frameSize / 2
                    PivotY = $(if ($row -eq 0) { 0 } else { 7 })
                    PixelsPerUnit = 1.8
                })
            }
        }
        $graphics.Dispose()
        $input.Dispose()
        $atlasName = $skin + $directions[$direction]
        $atlas.Save((Join-Path $output ($atlasName + '.png')), [System.Drawing.Imaging.ImageFormat]::Png)
        $atlas.Dispose()

        $pathPrefix = 'actors/species/civs/mclsl_xuanhuang' + $directions[$direction] + '/'
        $skins = if ($skin -eq 'male_1') { @('male_1', 'warrior_1', 'king', 'leader', 'child', 'zombie') }
            else { @('female_1') }
        foreach ($alias in $skins) {
            $ppu = if ($alias -eq 'child') { 2.5 } else { 1.8 }
            $aliasFrames = foreach ($frame in $frames) {
                $copy = @{} + $frame
                $copy.PixelsPerUnit = $ppu
                $copy
            }
            $manifest.Add(@{ Path = $pathPrefix + $alias; Atlas = 'actors/Xuanhuang/' + $atlasName; Frames = @($aliasFrames) })
        }
    }
}

$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $root 'resources/living-resources.json') -Encoding utf8
