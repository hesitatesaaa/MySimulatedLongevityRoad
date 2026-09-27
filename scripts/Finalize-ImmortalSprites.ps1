Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $PSScriptRoot 'SourceArt'
$actorRoot = Join-Path $projectRoot 'GameResources\actors\Immortals'
$traitRoot = Join-Path $projectRoot 'GameResources\trait'

foreach ($character in @('Bai')) {
    foreach ($state in @('Idle', 'Walk')) {
        for ($frame = 1; $frame -le 4; $frame++) {
            $relative = "$character\$state\$frame.png"
            $source = Join-Path $sourceRoot $relative
            $target = Join-Path $actorRoot $relative
            if (-not (Test-Path -LiteralPath $source)) { throw "Missing original sprite: $source" }
            $bitmap = [System.Drawing.Bitmap]::new($source)
            try {
                if ($bitmap.Width -ne 32 -or $bitmap.Height -ne 40) {
                    throw "Original sprite must be 32x40: $source"
                }
            }
            finally { $bitmap.Dispose() }
            [System.IO.Directory]::CreateDirectory((Split-Path -Parent $target)) | Out-Null
            Copy-Item -LiteralPath $source -Destination $target -Force
            if ($state -eq 'Idle' -and $frame -eq 1) {
                Copy-Item -LiteralPath $source -Destination (Join-Path $traitRoot "$character.png") -Force
            }
        }
    }
}

# Chuanfa's redrawn 4x2 sheet is the only source for his final frames.
# Do not copy scripts/SourceArt/Chuanfa, which contains the retired design.
$chuanfaSheet = Join-Path $sourceRoot 'ChuanfaTianzunSpriteSheet.png'
& (Join-Path $PSScriptRoot 'Prepare-ImmortalSprites.ps1') -OnlyCharacter Chuanfa -ChuanfaSheet $chuanfaSheet
