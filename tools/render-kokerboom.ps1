[CmdletBinding()]
param(
    [ValidatePattern('^round-[0-9]{2,3}$')][string]$Round = 'round-01',
    [int]$Seed = 4242,
    [ValidateRange(800,3840)][int]$Width = 1600,
    [switch]$ImportedCandidates,
    [switch]$Hybrid,
    [switch]$PlayablePreview,
    [switch]$PH02Crown,
    [switch]$PH01Tangents,
    [switch]$PH02Fitted,
    [switch]$PH02Family,
    [ValidateRange(0,1)][float]$PH02FoliageTint=0,
    [switch]$PH02ImportedTuples,
    [switch]$WoodDiagnostic,
    [switch]$ExactTupleDedup,
    [ValidateRange(0.001,1.419)][float]$PH02CutHeight = 0.65,
    [string]$EditorPath = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe',
    [ValidateRange(60,3600)][int]$TimeoutSeconds = 900
)
$ErrorActionPreference = 'Stop'
$projectPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not (Test-Path -LiteralPath (Join-Path $projectPath 'Assets\CityLife\Editor\KokerboomRender.cs'))) {
    throw 'This script must run from the cosmic CityLife worktree containing KokerboomRender.cs.'
}
if (-not (Test-Path -LiteralPath $EditorPath)) { throw "Pinned Unity editor was not found at $EditorPath" }
$runningEditors = @(Get-Process -Name Unity -ErrorAction SilentlyContinue)
if ($runningEditors.Count -gt 0) {
    throw 'A Unity editor is already running. Finish the coordinated editor operation before starting this isolated render; this script will not interrupt it.'
}
$outputDirectory = Join-Path $projectPath "evidence\milestones\kokerboom\$Round"
if ((Test-Path -LiteralPath $outputDirectory) -and @(Get-ChildItem -LiteralPath $outputDirectory -File).Count -gt 0) {
    throw "Evidence already exists in $Round. Choose a new round; existing captures are preserved."
}
$localLogs = Join-Path $projectPath 'evidence\local'
New-Item -ItemType Directory -Force -Path $localLogs | Out-Null
$stamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')
$logPath = Join-Path $localLogs "kokerboom-$Round-$stamp.log"
if(([int]$ImportedCandidates.IsPresent+[int]$Hybrid.IsPresent+[int]$PlayablePreview.IsPresent+[int]$PH02Crown.IsPresent+[int]$PH01Tangents.IsPresent+[int]$PH02Fitted.IsPresent+[int]$WoodDiagnostic.IsPresent+[int]$PH02Family.IsPresent) -gt 1){throw 'Select one inspection mode.'}
if($PSBoundParameters.ContainsKey('PH02FoliageTint') -and -not $PH02Family){throw 'PH02FoliageTint applies only to the PH02Family comparison.'}
if($ExactTupleDedup -and -not $PH01Tangents){throw 'ExactTupleDedup applies only to the PH01Tangents comparison.'}
if($PH02ImportedTuples -and -not $PH02Fitted){throw 'PH02ImportedTuples applies only to PH02Fitted; the default source path is unchanged.'}
if($PH02ImportedTuples -and $Width -ne 1600){throw 'PH02ImportedTuples retains the preserved R13 camera framing and 1600-pixel image width.'}
if($PH02Fitted -and $PSBoundParameters.ContainsKey('PH02CutHeight')){throw 'PH02Fitted uses the recorded source cut at0.65m; the free cut parameter belongs to PH02Crown.'}
if($WoodDiagnostic -and $Seed -ne 4242){throw 'WoodDiagnostic preserves the R09 seed4242 main specimen.'}
$entryPoint = if($PH02Family) { 'CityLife.World.Editor.KokerboomRender.RenderPH02Family' } elseif($WoodDiagnostic) { 'CityLife.World.Editor.KokerboomRender.RenderWoodDiagnostic' } elseif($PH02Fitted) { 'CityLife.World.Editor.KokerboomRender.RenderPH02FittedSupport' } elseif($PH01Tangents) { 'CityLife.World.Editor.KokerboomRender.RenderPH01TangentComparison' } elseif($PH02Crown) { 'CityLife.World.Editor.KokerboomRender.RenderPH02CrownCandidate' } elseif($PlayablePreview) { 'CityLife.World.Editor.KokerboomRender.BuildPlayablePreview' } elseif ($ImportedCandidates) { 'CityLife.World.Editor.KokerboomRender.RenderImportedCandidates' } elseif($Hybrid) { 'CityLife.World.Editor.KokerboomRender.RenderHybridFamily' } else { 'CityLife.World.Editor.KokerboomRender.RenderBatch' }
$expectedCaptures = if($PH02Family) { 21 } elseif($WoodDiagnostic) { 3 } elseif($PH02Fitted) { 12 } elseif($PH01Tangents -or $PH02Crown) { 8 } elseif($PlayablePreview) { 2 } elseif ($ImportedCandidates) { 12 } elseif($Hybrid) { 21 } else { 15 }
$arguments = @('-batchmode', '-force-d3d11', '-projectPath', ('"' + $projectPath + '"'),
    '-executeMethod', $entryPoint,
    '-kokerboomRound', $Round, '-kokerboomSeed', $Seed.ToString([Globalization.CultureInfo]::InvariantCulture),
    '-kokerboomWidth', $Width.ToString([Globalization.CultureInfo]::InvariantCulture),
    '-logFile', ('"' + $logPath + '"'))
if($PH02Crown){$arguments+=@('-ph02CutHeight',$PH02CutHeight.ToString('R',[Globalization.CultureInfo]::InvariantCulture))}
if($PH01Tangents){$arguments+=@('-ph01ExactTupleDedup',([int]$ExactTupleDedup.IsPresent).ToString([Globalization.CultureInfo]::InvariantCulture))}
if($PH02Fitted){$arguments+=@('-ph02ImportedTuples',([int]$PH02ImportedTuples.IsPresent).ToString([Globalization.CultureInfo]::InvariantCulture))}
if($PH02Family){$arguments+=@('-ph02FoliageTint',$PH02FoliageTint.ToString('R',[Globalization.CultureInfo]::InvariantCulture))}
# Graphics remain enabled. Batch mode + a hidden process do not activate a desktop editor window.
# The render entry point exits the process itself; -quit and -nographics are deliberately absent.
$renderProcess = Start-Process -FilePath $EditorPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
while (-not $renderProcess.WaitForExit(1000)) {
    if ([DateTime]::UtcNow -gt $deadline) {
        Stop-Process -Id $renderProcess.Id -Force
        throw "The isolated render exceeded $TimeoutSeconds seconds. Only the process started by this script was stopped. Inspect $logPath"
    }
}
$renderProcess.Refresh()
$exitCode = $renderProcess.ExitCode
$metricsPath = Join-Path $outputDirectory 'metrics.json'
if ($exitCode -ne 0 -or -not (Test-Path -LiteralPath $metricsPath)) {
    throw "Kokerboom rendering failed (exit $exitCode). Existing evidence is retained. Inspect $logPath"
}
$report = Get-Content -Raw -LiteralPath $metricsPath | ConvertFrom-Json
if($PH02Family -and ($report.mode -ne 'hybrid-ph02-fitted-crown-family-experiment' -or -not $report.ph02FamilyChecks.numericChecksPassed -or -not $report.ph02FamilyChecks.actualImportedGatePassed -or -not $report.ph02FamilyMeshReadbackPassed -or $report.ph02FoliageTintStrength -ne $PH02FoliageTint)){
    throw 'PH02 family component or requested tint did not pass the recorded technical checks. Component checks do not constitute full-family acceptance.'
}
if($PH02Fitted -and ($report.mode -ne 'ph02-fitted-support-comparison' -or -not $report.ph02FittedChecks.numericChecksPassed -or -not $report.ph02FittedReadback.passed)){
    throw "PH02 fitted support failed its actual Create/shared-rim readback checks. Inspect $metricsPath and ph02-fitted-support-checks.json."
}
if($PH02Fitted -and $report.ph02ImportedTuplesRequested -ne $PH02ImportedTuples.IsPresent){throw 'The requested PH02 crown input path was not recorded.'}
if($PH02ImportedTuples -and (-not $report.ph02ImportedCrownChecks.actualUnityImportedData -or -not $report.ph02ImportedCrownChecks.mappingComplete -or -not $report.ph02ImportedCrownChecks.exactExpandedTuplePreservation -or -not $report.ph02ImportedCrownMeshReadbackPassed -or -not $report.ph02FittedChecks.cloneMatchesCaller -or -not $report.ph02FittedChecks.callerCrownUnchanged)){
    throw 'Imported PH02 tuple mapping, actual Mesh expanded hashes or caller/clone preservation did not pass. Existing reports remain available.'
}
if($WoodDiagnostic -and $report.mode -ne 'r09-matched-wood-diagnostic'){throw 'The wood diagnostic mode was not recorded.'}
if($PH01Tangents){
    $expectedCaptures=if($report.originalSubsetImagesIncluded){12}else{8}
    if($report.mode -ne 'ph01-imported-tangent-comparison' -or -not $report.importedTupleChecks.numericChecksPassed -or $report.importedTupleChecks.exactTupleDedup -ne $ExactTupleDedup.IsPresent){
        throw "PH01 tuple checks or requested mode did not match. Numeric evidence is retained in $metricsPath and ph01-imported-tuple-checks.json."
    }
}
if (-not $report.technicalChecksPassed -or $report.expectedCaptures -ne $expectedCaptures -or @($report.captures).Count -ne $expectedCaptures) {
    throw "Render evidence did not pass its technical checks. Inspect $metricsPath and $logPath"
}
[pscustomobject]@{
    status = 'Rendered for independent critique'
    round = $Round
    images = @($report.captures).Count
    metrics = $metricsPath
    log = $logPath
    visualAcceptance = 'Not scored; independent critique required.'
} | ConvertTo-Json
