param([string]$Iteration = '01')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$renderDir = Join-Path $repo 'art-source/renders/trivia/round-01'
$names = @('wide_01','stage_02','reverse_03')
for ($i = 1; $i -le 3; $i++) {
    $refPath = Join-Path $repo ('art-source/concept/floors/trivia/trivia_tier1_game-show-' + $names[$i-1] + '.png')
    $renderPath = Join-Path $renderDir ('iteration-' + $Iteration + '_view-' + $i + '.png')
    if (-not (Test-Path -LiteralPath $renderPath)) { continue }
    $reference = [System.Drawing.Image]::FromFile($refPath)
    $render = [System.Drawing.Image]::FromFile($renderPath)
    $width = 1280
    $height = 720
    $canvas = New-Object System.Drawing.Bitmap(2560, 786)
    $graphics = [System.Drawing.Graphics]::FromImage($canvas)
    $graphics.Clear([System.Drawing.Color]::FromArgb(18,16,22))
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $font = New-Object System.Drawing.Font('Segoe UI', 17)
    $small = New-Object System.Drawing.Font('Segoe UI', 10)
    $graphics.DrawString('OWNER REFERENCE ' + $i, $font, [System.Drawing.Brushes]::White, 20, 7)
    $graphics.DrawString('BLENDER EEVEE | ITERATION ' + $Iteration, $font, [System.Drawing.Brushes]::White, 1300, 7)
    $graphics.DrawImage($reference, 0, 42, $width, $height)
    $graphics.DrawImage($render, 1280, 42, $width, $height)
    $graphics.DrawString('Round 1 intentionally omits people, text, confetti and light-beam effects. Image 1 controls conflicting geometry.', $small, [System.Drawing.Brushes]::Silver, 20, 765)
    $target = Join-Path $renderDir ('comparison-' + $Iteration + '_view-' + $i + '.jpg')
    $canvas.Save($target, [System.Drawing.Imaging.ImageFormat]::Jpeg)
    $graphics.Dispose(); $canvas.Dispose(); $font.Dispose(); $small.Dispose(); $reference.Dispose(); $render.Dispose()
    Write-Output $target
}
