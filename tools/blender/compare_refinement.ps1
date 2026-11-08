param([string]$Iteration='21')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$original=Join-Path $repo 'art-source/renders/trivia/round-01'
$review=Join-Path $repo 'art-source/renders/trivia/round-03'
$codec=[Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object MimeType -eq 'image/jpeg'
$parameters=New-Object Drawing.Imaging.EncoderParameters(1)
$parameters.Param[0]=New-Object Drawing.Imaging.EncoderParameter([Drawing.Imaging.Encoder]::Quality,[long]94)
function Pair([string]$left,[string]$right,[string]$leftLabel,[string]$rightLabel,[string]$output) {
    $a=[Drawing.Image]::FromFile($left);$b=[Drawing.Image]::FromFile($right)
    $canvas=New-Object Drawing.Bitmap(2560,768)
    $g=[Drawing.Graphics]::FromImage($canvas);$g.Clear([Drawing.Color]::FromArgb(18,16,22))
    $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $font=New-Object Drawing.Font('Segoe UI',17)
    $g.DrawString($leftLabel,$font,[Drawing.Brushes]::White,15,8)
    $g.DrawString($rightLabel,$font,[Drawing.Brushes]::White,1295,8)
    $g.DrawImage($a,0,48,1280,720);$g.DrawImage($b,1280,48,1280,720)
    $canvas.Save($output,$codec,$parameters)
    $g.Dispose();$canvas.Dispose();$font.Dispose();$a.Dispose();$b.Dispose()
}
for ($i=1;$i -le 3;$i++) {
    Pair (Join-Path $original "iteration-16_view-$i.png") (Join-Path $original "iteration-${Iteration}_view-$i.png") 'BEFORE | ITERATION 16' "REFINED | ITERATION $Iteration | SAME CAMERA / LAYOUT" (Join-Path $review "before-after-view-$i.jpg")
    $unity=Join-Path $review "unity-reference-view-$i.png"
    if (Test-Path -LiteralPath $unity) {
        Pair (Join-Path $original "iteration-${Iteration}_view-$i.png") $unity "BLENDER EEVEE | ITERATION $Iteration" 'UNITY URP | ACTUAL ART REVIEW SCENE | 4x MSAA + SMAA' (Join-Path $review "engine-comparison-view-$i.jpg")
    }
}
foreach ($name in @('bouquet','curtain','carving')) {
    $path=Join-Path $review "final_$name.png"
    if (Test-Path -LiteralPath $path) {
        $img=[Drawing.Image]::FromFile($path)
        $img.Save((Join-Path $review "detail-$name.jpg"),$codec,$parameters)
        $img.Dispose()
    }
}
$beforeLighting=Join-Path $review 'unity-pre-lighting-fix-view-2.png'
$afterLighting=Join-Path $review 'unity-reference-view-2.png'
if ((Test-Path -LiteralPath $beforeLighting) -and (Test-Path -LiteralPath $afterLighting)) {
    Pair $beforeLighting $afterLighting 'UNITY BEFORE | REFLECTION ASSIGNMENTS LOST ON RELOAD' 'UNITY AFTER | SAVED CUBEMAPS + REBALANCED LIGHTS' (Join-Path $review 'unity-lighting-before-after.jpg')
}
$third=Join-Path $review 'unity-playtest-third-person.png'
$first=Join-Path $review 'unity-playtest-first-person.png'
if ((Test-Path -LiteralPath $third) -and (Test-Path -LiteralPath $first)) {
    Pair $third $first 'UNITY AUTOMATED WALKTHROUGH | THIRD PERSON' 'UNITY AUTOMATED WALKTHROUGH | FIRST PERSON' (Join-Path $review 'playtest-both-views.jpg')
}
$parameters.Dispose()
