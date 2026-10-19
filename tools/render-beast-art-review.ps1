param([Parameter(Mandatory=$true)][string]$SourceDirectory,[Parameter(Mandatory=$true)][string]$DestinationDirectory)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Path $DestinationDirectory -Force | Out-Null
# Opaque QA previews apply alpha for accurate visual inspection. Never installed as art.
foreach($taskFile in Get-ChildItem -LiteralPath $SourceDirectory -Filter *.png){
 $taskInput=[Drawing.Bitmap]::new($taskFile.FullName)
 $taskPreview=[Drawing.Bitmap]::new($taskInput.Width,$taskInput.Height)
 $taskGraphics=[Drawing.Graphics]::FromImage($taskPreview)
 try {
  $taskGraphics.Clear([Drawing.Color]::FromArgb(255,30,32,36))
  $taskGraphics.DrawImageUnscaled($taskInput,0,0)
  $taskPreview.Save((Join-Path ([IO.Path]::GetFullPath($DestinationDirectory)) $taskFile.Name),[Drawing.Imaging.ImageFormat]::Png)
 } finally {$taskGraphics.Dispose();$taskInput.Dispose();$taskPreview.Dispose()}
}
