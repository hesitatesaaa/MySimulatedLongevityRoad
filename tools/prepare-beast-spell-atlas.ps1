param([Parameter(Mandatory=$true)][string]$Id,[Parameter(Mandatory=$true)][string]$Source,[ValidateRange(1,100)][int]$Revision)
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest=Get-Content -LiteralPath (Join-Path $taskRoot 'resources/beast-spell-effects.manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$entries=@($manifest.assets | Where-Object id -eq $Id)
if($entries.Count -ne 1){throw 'Unknown or ambiguous stage ID'}
$entry=$entries[0];$columns=[int]$entry.columns;$rows=[int][Math]::Ceiling($entry.frames/$columns)
$candidateDirectory=Join-Path $taskRoot 'output/beast-overhaul/spell-candidates'
New-Item -ItemType Directory -Path $candidateDirectory -Force | Out-Null
$destination=Join-Path $candidateDirectory ($Id+'-v'+$Revision+'.png')
if(Test-Path -LiteralPath $destination){throw 'Candidate already exists'}
Add-Type -AssemblyName System.Drawing
$inputImage=[Drawing.Bitmap]::new([IO.Path]::GetFullPath($Source));$outputImage=$null;$graphics=$null
try {
 if([Math]::Abs(($inputImage.Width/$inputImage.Height)-($columns/$rows)) -gt 0.02){throw 'Source aspect ratio does not match declared grid'}
 $outputImage=[Drawing.Bitmap]::new([int]$entry.width,[int]$entry.height,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $graphics=[Drawing.Graphics]::FromImage($outputImage);$graphics.Clear([Drawing.Color]::Transparent)
 $graphics.CompositingMode=[Drawing.Drawing2D.CompositingMode]::SourceCopy
 $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 $graphics.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::HighQuality
 # One fixed scale and source cell center preserve temporal size changes and pivots.
 # The 16px transparent border is output-format padding, never a per-frame crop.
 for($index=0;$index -lt $entry.frames;$index++){
  $column=$index%$columns;$row=[Math]::Floor($index/$columns)
  $sourceRect=[Drawing.RectangleF]::new([single]($column*$inputImage.Width/$columns),[single]($row*$inputImage.Height/$rows),[single]($inputImage.Width/$columns),[single]($inputImage.Height/$rows))
  $targetRect=[Drawing.RectangleF]::new([single]($column*192+16),[single]($row*192+16),160,160)
  $graphics.DrawImage($inputImage,$targetRect,$sourceRect,[Drawing.GraphicsUnit]::Pixel)
 }
 $outputImage.Save($destination,[Drawing.Imaging.ImageFormat]::Png)
 [pscustomobject]@{id=$Id;candidate=$destination;frames=$entry.frames;columns=$columns;paddingPixels=16;review='pending'} | ConvertTo-Json -Compress
} finally {
 if($graphics){$graphics.Dispose()};if($outputImage){$outputImage.Dispose()};$inputImage.Dispose()
}
