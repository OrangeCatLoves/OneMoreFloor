param([switch]$UseCurrentBuild)
$ErrorActionPreference='Stop'
$repo=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $repo
$blender='C:/Program Files/Blender Foundation/Blender 5.2/blender.exe'
# Native Blender writes benign thumbnail diagnostics to stderr on this machine.
$ErrorActionPreference='Continue'
if (-not $UseCurrentBuild) {
    & $blender -b --python-exit-code 1 --python tools/blender/resume_trivia_round_two.py -- --iteration 23 *> art-source/trivia/build-round5-layout.log
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
& $blender -b --python-exit-code 1 --python tools/blender/audit_trivia_round_two.py *> art-source/trivia/round5-source-audit.log
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
unity run . --timeout 900 --log-file art-source/trivia/round5-unity-build.log --no-tail -- -executeMethod OneMoreFloor.ArtReview.TriviaReviewSceneBuilder.BuildAndCapture
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
unity run . --timeout 240 --log-file art-source/trivia/round5-after.log --no-tail -- -executeMethod OneMoreFloor.ArtReview.TriviaRoundTwoReview.After
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $blender -b --python-exit-code 1 --python tools/blender/audit_trivia.py *> art-source/trivia/round5-general-audit.log
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
unity test . --mode EditMode --output art-source/trivia/round5-edit-results.xml --timeout 300
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
unity test . --mode PlayMode --output art-source/trivia/round5-play-results.xml --timeout 600
exit $LASTEXITCODE
