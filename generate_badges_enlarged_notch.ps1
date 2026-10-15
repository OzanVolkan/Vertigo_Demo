Add-Type -AssemblyName System.Drawing

$targetDir = "c:\Users\ozana\Desktop\Unity Projects\Vertigo_Demo\Assets\2D\demo_content"

function Generate-Badge {
    param(
        [string]$filePath,
        [System.Drawing.Color[]]$gradientColors,
        [float[]]$gradientPositions,
        [System.Drawing.Color]$borderColorBottom,
        [System.Drawing.Color]$shadowColor
    )

    $w = 120
    $h = 136
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $r = 18.0
    $rect = New-Object System.Drawing.RectangleF 6.0, 8.0, ($w - 12.0), ($h - 16.0)
    $cx = $w / 2.0

    # Build continuous rounded rectangle
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2.0
    $path.AddArc($rect.X, $rect.Y, $d, $d, 180, 90)
    $path.AddArc(($rect.Right - $d), $rect.Y, $d, $d, 270, 90)
    $path.AddArc(($rect.Right - $d), ($rect.Bottom - $d), $d, $d, 0, 90)
    $path.AddArc($rect.X, ($rect.Bottom - $d), $d, $d, 90, 90)
    $path.CloseFigure()

    # Fill base gradient
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush (
        (New-Object System.Drawing.PointF 0, $rect.Top),
        (New-Object System.Drawing.PointF 0, $rect.Bottom),
        $gradientColors[0],
        $gradientColors[$gradientColors.Length - 1]
    )
    $cb = New-Object System.Drawing.Drawing2D.ColorBlend $gradientColors.Length
    $cb.Colors = $gradientColors
    $cb.Positions = $gradientPositions
    $brush.InterpolationColors = $cb
    $g.FillPath($brush, $path)
    $brush.Dispose()

    # Outer border pen (pure white top to subtle colored bottom)
    $pen = New-Object System.Drawing.Pen (
        (New-Object System.Drawing.Drawing2D.LinearGradientBrush (
            (New-Object System.Drawing.PointF 0, $rect.Top),
            (New-Object System.Drawing.PointF 0, $rect.Bottom),
            [System.Drawing.Color]::FromArgb(255, 255, 255, 255),
            $borderColorBottom
        )), 4.0
    )
    $g.DrawPath($pen, $path)
    $pen.Dispose()

    # Inner subtle highlight along the perimeter
    $innerRect = New-Object System.Drawing.RectangleF ($rect.X + 2.5), ($rect.Y + 2.5), ($rect.Width - 5.0), ($rect.Height - 5.0)
    $innerPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $id = ($r - 2.5) * 2.0
    $innerPath.AddArc($innerRect.X, $innerRect.Y, $id, $id, 180, 90)
    $innerPath.AddArc(($innerRect.Right - $id), $innerRect.Y, $id, $id, 270, 90)
    $innerPath.AddArc(($innerRect.Right - $id), ($innerRect.Bottom - $id), $id, $id, 0, 90)
    $innerPath.AddArc($innerRect.X, ($innerRect.Bottom - $id), $id, $id, 90, 90)
    $innerPath.CloseFigure()

    $innerPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(60, 255, 255, 255), 1.5)
    $g.DrawPath($innerPen, $innerPath)
    $innerPen.Dispose()
    $innerPath.Dispose()

    # Enlarged Inverted Triangle (Pointer Notch) at top center
    # Upgraded from 28x17 -> 40x25 (+43% width, +47% height)
    $notchW = 40.0
    $notchH = 25.0
    $notchTopY = $rect.Top + 1.0

    # Triangle dark drop shadow backdrop
    $shadowPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $shadowPath.AddPolygon(@(
        (New-Object System.Drawing.PointF ($cx - $notchW / 2.0 - 2.5), ($notchTopY - 1.0)),
        (New-Object System.Drawing.PointF ($cx + $notchW / 2.0 + 2.5), ($notchTopY - 1.0)),
        (New-Object System.Drawing.PointF $cx, ($notchTopY + $notchH + 3.5))
    ))
    $shadowBrush = New-Object System.Drawing.SolidBrush $shadowColor
    $g.FillPath($shadowBrush, $shadowPath)
    $shadowBrush.Dispose()
    $shadowPath.Dispose()

    # Triangle solid white fill
    $notchPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $notchPath.AddPolygon(@(
        (New-Object System.Drawing.PointF ($cx - $notchW / 2.0), $notchTopY),
        (New-Object System.Drawing.PointF ($cx + $notchW / 2.0), $notchTopY),
        (New-Object System.Drawing.PointF $cx, ($notchTopY + $notchH))
    ))
    $notchBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
    $g.FillPath($notchBrush, $notchPath)
    $notchBrush.Dispose()

    # White outline around the triangle
    $notchPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 255, 255, 255), 1.5)
    $g.DrawPath($notchPen, $notchPath)
    $notchPen.Dispose()
    $notchPath.Dispose()

    $path.Dispose()
    $g.Dispose()

    $bmp.Save($filePath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Output "Generated badge: $filePath"
}

# 1. Normal (Silver Metallic)
Generate-Badge `
    -filePath (Join-Path $targetDir "ui_badge_current_zone.png") `
    -gradientColors @(
        [System.Drawing.Color]::FromArgb(255, 218, 222, 228),
        [System.Drawing.Color]::FromArgb(255, 205, 210, 218),
        [System.Drawing.Color]::FromArgb(255, 175, 180, 190),
        [System.Drawing.Color]::FromArgb(255, 142, 147, 158)
    ) `
    -gradientPositions @(0.0, 0.30, 0.70, 1.0) `
    -borderColorBottom ([System.Drawing.Color]::FromArgb(255, 185, 190, 200)) `
    -shadowColor ([System.Drawing.Color]::FromArgb(120, 60, 65, 75))

# 2. Safe (Vivid Green)
Generate-Badge `
    -filePath (Join-Path $targetDir "ui_badge_current_zone_safe.png") `
    -gradientColors @(
        [System.Drawing.Color]::FromArgb(255, 115, 205, 38),
        [System.Drawing.Color]::FromArgb(255, 88, 175, 24),
        [System.Drawing.Color]::FromArgb(255, 52, 126, 14),
        [System.Drawing.Color]::FromArgb(255, 26, 75, 7)
    ) `
    -gradientPositions @(0.0, 0.35, 0.75, 1.0) `
    -borderColorBottom ([System.Drawing.Color]::FromArgb(255, 225, 235, 225)) `
    -shadowColor ([System.Drawing.Color]::FromArgb(130, 15, 45, 5))

# 3. Super (Metallic Gold)
Generate-Badge `
    -filePath (Join-Path $targetDir "ui_badge_current_zone_super.png") `
    -gradientColors @(
        [System.Drawing.Color]::FromArgb(255, 255, 235, 115),
        [System.Drawing.Color]::FromArgb(255, 255, 195, 25),
        [System.Drawing.Color]::FromArgb(255, 210, 145, 12),
        [System.Drawing.Color]::FromArgb(255, 145, 92, 5)
    ) `
    -gradientPositions @(0.0, 0.35, 0.75, 1.0) `
    -borderColorBottom ([System.Drawing.Color]::FromArgb(255, 245, 230, 180)) `
    -shadowColor ([System.Drawing.Color]::FromArgb(130, 80, 45, 5))
