param(
    [string]$BaiSheet,
    [string]$ChuanfaSheet,
    [ValidateSet('Bai', 'Chuanfa', 'Both')][string]$OnlyCharacter = 'Both'
)

Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$assetRoot = Join-Path $PSScriptRoot '..\GameResources\actors\Immortals'
$characters = @(
    @{ Name = 'Bai'; Sheet = $BaiSheet },
    @{ Name = 'Chuanfa'; Sheet = $ChuanfaSheet }
) | Where-Object { $OnlyCharacter -eq 'Both' -or $_.Name -eq $OnlyCharacter }
foreach ($character in $characters) {
    if (-not (Test-Path -LiteralPath $character.Sheet)) {
        throw "Missing sprite sheet for $($character.Name): $($character.Sheet)"
    }
    $source = [System.Drawing.Bitmap]::new($character.Sheet)
    try {
        $cellWidth = [int][Math]::Floor($source.Width / 4)
        $cellHeight = [int][Math]::Floor($source.Height / 2)
        for ($frame = 0; $frame -lt 8; $frame++) {
            $row = [int][Math]::Floor($frame / 4)
            $col = $frame % 4
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
            if ($right -le $left -or $bottom -le $top) { throw "Empty sprite frame $($character.Name) $frame" }
            $width = $right - $left + 3
            $height = $bottom - $top + 3
            $scale = [Math]::Min(30.0 / $width, 38.0 / $height)
            $drawWidth = [Math]::Max(1, [int][Math]::Round($width * $scale))
            $drawHeight = [Math]::Max(1, [int][Math]::Round($height * $scale))
            $output = [System.Drawing.Bitmap]::new(32, 40)
            try {
                $graphics = [System.Drawing.Graphics]::FromImage($output)
                try {
                    $graphics.Clear([System.Drawing.Color]::Transparent)
                    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                    $destination = [System.Drawing.Rectangle]::new([int][Math]::Floor((32 - $drawWidth) / 2), 40 - $drawHeight, $drawWidth, $drawHeight)
                    $crop = [System.Drawing.Rectangle]::new($left, $top, $width, $height)
                    $graphics.DrawImage($source, $destination, $crop, [System.Drawing.GraphicsUnit]::Pixel)
                }
                finally { $graphics.Dispose() }
                $folder = if ($frame -lt 4) { 'Idle' } else { 'Walk' }
                $index = if ($frame -lt 4) { $frame + 1 } else { $frame - 3 }
                $targetDir = Join-Path $assetRoot "$($character.Name)\$folder"
                New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
                $output.Save((Join-Path $targetDir "$index.png"), [System.Drawing.Imaging.ImageFormat]::Png)
                if ($frame -eq 0) {
                    $traitDir = Join-Path $PSScriptRoot '..\GameResources\trait'
                    $output.Save((Join-Path $traitDir "$($character.Name).png"), [System.Drawing.Imaging.ImageFormat]::Png)
                }
            }
            finally { $output.Dispose() }
        }
    }
    finally { $source.Dispose() }
}
