param([string]$Iteration = '16', [switch]$Downsample, [switch]$Evidence)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$renders = Join-Path $repo 'art-source/renders/trivia/round-01'
$review = Join-Path $repo 'art-source/renders/trivia/round-02'

if ($Downsample) {
    for ($i=1; $i -le 3; $i++) {
        $path = Join-Path $renders ('iteration-' + $Iteration + '_view-' + $i + '.png')
        $original = [Drawing.Image]::FromFile($path)
        if ($original.Width -eq 1672) { $original.Dispose(); continue }
        if ($original.Width -ne 3344 -or $original.Height -ne 1882) { $original.Dispose(); throw "Unexpected source dimensions: $path" }
        $original.Dispose()
        $raw = Join-Path $renders ('supersampled-' + $Iteration + '_view-' + $i + '.png')
        Copy-Item -LiteralPath $path -Destination $raw -Force
        $original = [Drawing.Image]::FromFile($raw)
        $canvas = New-Object Drawing.Bitmap(1672,941)
        $graphics = [Drawing.Graphics]::FromImage($canvas)
        $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
        $attributes = New-Object Drawing.Imaging.ImageAttributes
        $attributes.SetWrapMode([Drawing.Drawing2D.WrapMode]::TileFlipXY)
        $graphics.DrawImage($original, [Drawing.Rectangle]::new(0,0,1672,941), 0,0,3344,1882,[Drawing.GraphicsUnit]::Pixel,$attributes)
        $canvas.Save($path,[Drawing.Imaging.ImageFormat]::Png)
        $attributes.Dispose();$graphics.Dispose();$canvas.Dispose();$original.Dispose()
        Write-Output "2x supersampling resolved to $path"
    }
}

if ($Evidence) {
    $off = [Drawing.Image]::FromFile((Join-Path $review 'unity-aa-off.png'))
    $on = [Drawing.Image]::FromFile((Join-Path $review 'unity-aa-on.png'))
    # Exact pixel comparison: enlarge with nearest-neighbour so the compositor
    # itself cannot hide aliasing. Same camera/geometry/lighting in both inputs.
    $crops = @([Drawing.Rectangle]::new(300,20,400,170), [Drawing.Rectangle]::new(40,200,400,170), [Drawing.Rectangle]::new(830,425,400,170))
    $labels = @('Lift arch and gold trim','Curved flower petals and leaves','Diagonal stage edge and board seams')
    $canvas = New-Object Drawing.Bitmap(1600,1190)
    $graphics = [Drawing.Graphics]::FromImage($canvas)
    $graphics.Clear([Drawing.Color]::FromArgb(20,17,24))
    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
    $font = New-Object Drawing.Font('Segoe UI',17)
    $small = New-Object Drawing.Font('Segoe UI',12)
    $graphics.DrawString('AA OFF | native 720p source', $font, [Drawing.Brushes]::White, 12,8)
    $graphics.DrawString('4x MSAA + SMAA HIGH | same scene', $font, [Drawing.Brushes]::White, 812,8)
    for ($row=0; $row -lt 3; $row++) {
        $y = 48+$row*374
        $graphics.DrawString($labels[$row], $small, [Drawing.Brushes]::LightGray,12,$y)
        $graphics.DrawImage($off,[Drawing.Rectangle]::new(0,$y+27,800,340),$crops[$row],[Drawing.GraphicsUnit]::Pixel)
        $graphics.DrawImage($on,[Drawing.Rectangle]::new(800,$y+27,800,340),$crops[$row],[Drawing.GraphicsUnit]::Pixel)
    }
    $graphics.DrawString('200% nearest-neighbour enlargement; no resampling blur. AA reduces pixel stair-steps; it cannot repair a polygon silhouette.', $small, [Drawing.Brushes]::LightGray,12,1168)
    $canvas.Save((Join-Path $review 'aa-detail-comparison.png'),[Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose();$canvas.Dispose();$font.Dispose();$small.Dispose();$off.Dispose();$on.Dispose()

    $third = [Drawing.Image]::FromFile((Join-Path $review 'unity-playtest-third-person.png'))
    $first = [Drawing.Image]::FromFile((Join-Path $review 'unity-playtest-first-person.png'))
    $canvas = New-Object Drawing.Bitmap(2560,760)
    $graphics = [Drawing.Graphics]::FromImage($canvas)
    $graphics.Clear([Drawing.Color]::FromArgb(20,17,24))
    $font = New-Object Drawing.Font('Segoe UI',17)
    $graphics.DrawString('UNITY PLAYTEST | THIRD PERSON | 4x MSAA + SMAA', $font, [Drawing.Brushes]::White,12,6)
    $graphics.DrawString('UNITY PLAYTEST | FIRST PERSON | 4x MSAA + SMAA', $font, [Drawing.Brushes]::White,1292,6)
    $graphics.DrawImageUnscaled($third,0,40)
    $graphics.DrawImageUnscaled($first,1280,40)
    $codec = [Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object MimeType -eq 'image/jpeg'
    $parameters = New-Object Drawing.Imaging.EncoderParameters(1)
    $parameters.Param[0] = New-Object Drawing.Imaging.EncoderParameter([Drawing.Imaging.Encoder]::Quality, [long]95)
    $canvas.Save((Join-Path $review 'playtest-both-views.jpg'),$codec,$parameters)
    $parameters.Dispose();$graphics.Dispose();$canvas.Dispose();$font.Dispose();$third.Dispose();$first.Dispose()
}
