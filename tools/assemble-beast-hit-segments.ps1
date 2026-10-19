param([Parameter(Mandatory=$true)][string]$Id,[ValidateRange(1,100)][int]$Revision=5,[ValidateRange(1,100)][int]$PartRevision=5,[ValidateRange(1,100)][int]$TailRevision=5)
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskManifest=Get-Content -LiteralPath (Join-Path $taskRoot 'resources/beast-spell-effects.manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$taskEntry=@($taskManifest.assets | Where-Object id -eq $Id)
if($taskEntry.Count -ne 1 -or $taskEntry[0].frames -ne 24){throw 'Only a declared 24-frame stage can use three 8-frame segments'}
Add-Type -AssemblyName System.Drawing
$taskImages=@();$taskReceipts=@();$taskOutput=$null;$taskGraphics=$null
$taskTarget=Join-Path $taskRoot ('output/beast-overhaul/generated-spell-originals/'+$Id+'-v'+$Revision+'.png')
if(Test-Path -LiteralPath $taskTarget){throw 'Original composite already exists'}
try{
 for($taskPart=1;$taskPart -le 3;$taskPart++){
  $taskPartId=$Id+'_p'+$taskPart
  $taskSourceRevision=if($taskPart -eq 3){$TailRevision}else{$PartRevision}
  $taskReceipt=Get-Content -LiteralPath (Join-Path $taskRoot ('output/beast-overhaul/receipts/'+$taskPartId+'-v'+$taskSourceRevision+'.json')) -Raw -Encoding UTF8 | ConvertFrom-Json
  $taskImage=[Drawing.Bitmap]::new((Join-Path $taskRoot $taskReceipt.source))
  if([Math]::Abs($taskImage.Width/$taskImage.Height-2) -gt 0.02){$taskImage.Dispose();throw 'Segment grid must be four columns by two rows'}
  $taskImages+=$taskImage;$taskReceipts+=$taskReceipt
 }
 $taskWidth=[int](($taskImages | Measure-Object Width -Maximum).Maximum)
 $taskHeight=[int][Math]::Round($taskWidth/2)
 $taskOutput=[Drawing.Bitmap]::new($taskWidth,($taskHeight*3),[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $taskGraphics=[Drawing.Graphics]::FromImage($taskOutput);$taskGraphics.Clear([Drawing.Color]::Transparent)
 $taskGraphics.CompositingMode=[Drawing.Drawing2D.CompositingMode]::SourceCopy
 $taskGraphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 for($taskPart=0;$taskPart -lt 3;$taskPart++){
  $taskGraphics.DrawImage($taskImages[$taskPart],[Drawing.Rectangle]::new(0,($taskPart*$taskHeight),$taskWidth,$taskHeight))
 }
 $taskOutput.Save($taskTarget,[Drawing.Imaging.ImageFormat]::Png)
 $taskCombined=[ordered]@{id=$Id;revision=$Revision;kind='spell';tool='built-in-imagegen';source=('output/beast-overhaul/generated-spell-originals/'+$Id+'-v'+$Revision+'.png');sourceSha256=(Get-FileHash -LiteralPath $taskTarget -Algorithm SHA256).Hash.ToLowerInvariant();prompt=($taskReceipts.prompt -join ([Environment]::NewLine+[Environment]::NewLine));references=@($taskReceipts.references | Select-Object -Unique);parts=$taskReceipts;assembly=@{tool='System.Drawing';method='Three full four-column two-row sheets in chronological vertical order; no per-frame crop or size normalization';frames=24};generatedAtUtc=[DateTime]::UtcNow.ToString('o');status='candidate-unreviewed'}
 $taskReceiptText=$taskCombined | ConvertTo-Json -Depth 20
 [IO.File]::WriteAllText((Join-Path $taskRoot ('output/beast-overhaul/receipts/'+$Id+'-v'+$Revision+'.json')),$taskReceiptText,[Text.UTF8Encoding]::new($false))
 [pscustomobject]@{id=$Id;frames=24;saved=$true;review='pending'} | ConvertTo-Json -Compress
}finally{
 if($taskGraphics){$taskGraphics.Dispose()};if($taskOutput){$taskOutput.Dispose()};foreach($taskImage in $taskImages){$taskImage.Dispose()}
}