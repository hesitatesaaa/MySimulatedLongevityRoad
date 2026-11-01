$ErrorActionPreference = "Stop"
# 路径由脚本自身位置推导，避免硬编码本机绝对路径
$base = Join-Path (Split-Path -Parent $PSScriptRoot) "code"
Set-Location $base

$dirs = @(
  "Systems","Data","Traits","Patches","Interop"
)

function Rel([string]$p) {
  $full = [System.IO.Path]::GetFullPath($p)
  return $full.Substring($base.Length + 1).Replace("\","/")
}

# collect all in-scope cs files
$files = @()
foreach ($d in $dirs) {
  $root = Join-Path $base "MySimulatedLongevityRoad\$d"
  $files += Get-ChildItem -Recurse -Filter *.cs $root | ForEach-Object { $_.FullName }
}

$out = @()
$fileIdx = 0
foreach ($f in $files) {
  $fileIdx++
  $rel = Rel $f
  try {
    $j = & codegraph refs $f --json 2>$null | Out-String
    $obj = $j | ConvertFrom-Json
    if (-not $obj) { continue }
    $symfile = $obj[0] # { status, file, symbols }
    foreach ($sym in $symfile.symbols) {
      $def = $sym.definition
      if (-not $def.isMember) { continue }
      $kind = $def.kind
      # skip types/methods we can't cheaply reason about? Keep all; label kind.
      $rf = $sym.references
      if ($rf.status -ne "ok") { continue }
      $refFiles = @()
      foreach ($r in $rf.references) {
        $rf2 = $r.file
        # The reference file may be embedded. $r.file may be a string or object
        if ($rf2 -is [string]) { $refFiles += $rf2 }
      }
      # count distinct files not equal to owning file
      $external = @($refFiles | Where-Object { $_ -and (Rel $_) -ne $rel } | Select-Object -Unique)
      $intra = @($refFiles | Where-Object { $_ -and (Rel $_) -eq $rel })
      $localName = $def.localName
      $state = $rf.referenceCoverage.state
      $scope = $rf.referenceCoverage.scope
      $out += [pscustomobject]@{
        File = $rel
        Member = $localName
        Kind = $kind
        ExternalRefs = $external.Count
        IntraRefs = $intra.Count
        Coverage = "$scope/$state"
        Line = $def.range.start.line
      }
    }
  } catch {
    # skip unparseable
  }
}

$out | Export-Csv "D:\模拟长胜路\_refcounts.csv" -NoTypeInformation -Encoding UTF8
"Total member rows: $($out.Count)"
$zero = @($out | Where-Object { $_.ExternalRefs -eq 0 })
"Zero-external members: $($zero.Count)"
$zero | Where-Object { $_.IntraRefs -le 3 } | Sort-Object File,Line | Format-Table File,Line,Kind,Member,IntraRefs -AutoSize