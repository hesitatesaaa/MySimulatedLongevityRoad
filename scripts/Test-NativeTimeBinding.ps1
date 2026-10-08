[CmdletBinding()]
param(
    [string]$WorldBoxDataRoot = 'D:\owl\gameversion\0_51_2_imported\worldbox_Data',
    [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not [IO.Path]::IsPathRooted($WorldBoxDataRoot)) {
    $WorldBoxDataRoot = Join-Path $repoRoot $WorldBoxDataRoot
}
Add-Type -Path (Join-Path $WorldBoxDataRoot 'StreamingAssets\mods\NML\Assemblies\Mono.Cecil.dll')
$mod = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $repoRoot "bin\$Configuration\netstandard2.1\MySimulatedLongevityRoad.dll"))
try {
    $types = [Collections.Generic.Queue[object]]::new()
    foreach ($type in $mod.MainModule.Types) { $types.Enqueue($type) }
    while ($types.Count -gt 0) {
        $type = $types.Dequeue()
        foreach ($nested in $type.NestedTypes) { $types.Enqueue($nested) }
        foreach ($method in $type.Methods) {
            if (-not $method.HasBody) { continue }
            foreach ($instruction in $method.Body.Instructions) {
                $operand = [string]$instruction.Operand
                if ($type.FullName -match '\.Patches\.' -and
                    $operand -match 'updateSimulation|calculateCurElapsed|checkMainSimulationUpdate|updateWorldTime') {
                    throw "无降速约束失败：发现原生时间入口补丁或调用 $($method.FullName): $operand"
                }
                # Explicit player-opened windows retain their existing pause behavior.
                if ($type.FullName -match '\.UI\.') { continue }
                if ($operand -match 'Config::setWorldSpeed') {
                    throw "无降速约束失败：发现未经授权的原生速度调用 $($method.FullName): $operand"
                }
                if ($operand -match 'UnityEngine.Time::set_(timeScale|fixedDeltaTime)|Config::set_paused' -or
                    ($instruction.OpCode.Name -match '^st' -and
                    $operand -match 'Config::time_scale_asset|WorldTimeScaleAsset::(multiplier|ticks)|MapBox::elapsed')) {
                    throw "无降速约束失败：非界面代码改写原生速度或暂停 $($method.FullName): $operand"
                }
            }
        }
    }
    if (@($mod.MainModule.Types | Where-Object Name -eq 'MclslAnnualTimePolicy').Count -gt 0) {
        throw '旧年度降速策略仍在编译产物中。'
    }
    Write-Host '无降速约束检查通过：无年度控速、暂停写入或旧降速策略。'
}
finally { $mod.Dispose() }
