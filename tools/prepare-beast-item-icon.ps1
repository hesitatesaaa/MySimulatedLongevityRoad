param([Parameter(Mandatory=$true)][string]$Id,[Parameter(Mandatory=$true)][string]$Source,[ValidateRange(0,255)][int]$BoundsAlpha=16,[ValidateRange(0,100)][int]$Revision=0,[ValidateRange(0,100)][int]$OriginalRevision=0)
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest=Get-Content -LiteralPath (Join-Path $taskRoot 'resources/beast-content.manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$entry=@($manifest.icons | Where-Object id -eq $Id)
if($entry.Count -ne 1){throw 'Unknown or ambiguous item ID'}
$originalDirectory=Join-Path $taskRoot 'output/beast-overhaul/generated-originals'
$candidateDirectory=Join-Path $taskRoot 'output/beast-overhaul/icon-candidates'
New-Item -ItemType Directory -Path $originalDirectory,$candidateDirectory -Force | Out-Null
$originalSuffix=if($OriginalRevision -gt 0){'-v'+$OriginalRevision}else{''}
$original=Join-Path $originalDirectory ($Id+$originalSuffix+'.png')
if([IO.Path]::GetFullPath($Source) -ne [IO.Path]::GetFullPath($original)){
 if(Test-Path -LiteralPath $original){throw 'Original already exists; use a versioned ID workflow for replacement'}
 Copy-Item -LiteralPath $Source -Destination $original
}
$suffix=if($Revision -gt 0){'-v'+$Revision}else{''}
$destination=Join-Path $candidateDirectory ($Id+$suffix+'.png')
if(Test-Path -LiteralPath $destination){throw 'Candidate already exists; do not overwrite an audited candidate'}
Add-Type -AssemblyName System.Drawing
$inputImage=[Drawing.Bitmap]::new($original)
$outputImage=$null;$graphics=$null
try {
 # Format conversion for the requested 64x64 installation icons. Native art is preserved.
 $left=$inputImage.Width;$top=$inputImage.Height;$right=-1;$bottom=-1
 for($y=0;$y -lt $inputImage.Height;$y++){
  for($x=0;$x -lt $inputImage.Width;$x++){
   if($inputImage.GetPixel($x,$y).A -ge [Math]::Max(1,$BoundsAlpha)){
    $left=[Math]::Min($left,$x);$top=[Math]::Min($top,$y)
    $right=[Math]::Max($right,$x);$bottom=[Math]::Max($bottom,$y)
   }
  }
 }
 if($right -lt 0){throw 'Empty alpha image'}
 $sourceWidth=$right-$left+1;$sourceHeight=$bottom-$top+1
 $factor=56.0/[Math]::Max($sourceWidth,$sourceHeight)
 $width=[Math]::Max(1,[Math]::Round($sourceWidth*$factor));$height=[Math]::Max(1,[Math]::Round($sourceHeight*$factor))
 $outputImage=[Drawing.Bitmap]::new(64,64,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $graphics=[Drawing.Graphics]::FromImage($outputImage)
 $graphics.Clear([Drawing.Color]::Transparent)
 $graphics.CompositingMode=[Drawing.Drawing2D.CompositingMode]::SourceCopy
 $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 $graphics.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::HighQuality
 $target=[Drawing.Rectangle]::new([int]((64-$width)/2),[int]((64-$height)/2),$width,$height)
 $graphics.DrawImage($inputImage,$target,$left,$top,$sourceWidth,$sourceHeight,[Drawing.GraphicsUnit]::Pixel)
 $outputImage.Save($destination,[Drawing.Imaging.ImageFormat]::Png)
 [pscustomobject]@{id=$Id;original=$original;candidate=$destination;size='64x64';boundsAlpha=$BoundsAlpha;review='pending'} | ConvertTo-Json -Compress
} finally {
 if($graphics){$graphics.Dispose()};if($outputImage){$outputImage.Dispose()};$inputImage.Dispose()
}
