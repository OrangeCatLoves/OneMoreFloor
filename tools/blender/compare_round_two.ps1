$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$review=Join-Path $repo 'art-source/renders/trivia/round-05'
$codec=[Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object MimeType -eq 'image/jpeg'
$parameters=New-Object Drawing.Imaging.EncoderParameters(1)
$parameters.Param[0]=New-Object Drawing.Imaging.EncoderParameter([Drawing.Imaging.Encoder]::Quality,[long]92)
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
foreach ($before in Get-ChildItem -LiteralPath $review -Filter 'before-*.png') {
    $stem=$before.BaseName.Substring(7)
    $after=Join-Path $review ('after-'+$stem+'.png')
    if (-not (Test-Path -LiteralPath $after)) { throw ('Missing after view '+$stem) }
    Pair $before.FullName $after ('BEFORE | '+$stem+' | UNITY') ('ROUND 2 | SAME UNITY CAMERA | '+$stem) (Join-Path $review ($stem+'.jpg'))
}
Pair (Join-Path $review 'unity-playtest-first-person.png') (Join-Path $review 'unity-playtest-third-person.png') 'ROUND 2 | UNITY WALKTHROUGH | FIRST PERSON' 'ROUND 2 | UNITY WALKTHROUGH | THIRD PERSON' (Join-Path $review 'playtest-both-views.jpg')
$parameters.Dispose()
