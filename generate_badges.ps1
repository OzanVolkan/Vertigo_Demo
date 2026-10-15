Add-Type -AssemblyName System.Drawing

$demoDir = "c:\Users\ozana\Desktop\Unity Projects\Vertigo_Demo\Assets\2D\demo_content"

function New-RoundedRectanglePath {
    param(
        [float]$x, [float]$y, [float]$width, [float]$height, [float]$radius
    )
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $diameter = $radius * 2

    # Top-Left Arc
    $path.AddArc($x, $y, $diameter, $diameter, 180, 90)
    # Top Line
    $path.AddLine($x + $radius, $y, $x + $width - $radius, $y)
    # Top-Right Arc
    $path.AddArc($x + $width - $diameter, $y, $diameter, $diameter, 270, 90)
    # Right Line
    $path.AddLine($x + $width, $y + $radius, $x + $width, $y + $height - $radius)
    # Bottom-Right Arc
    $path.AddArc($x + $width - $diameter, $y + $height - $diameter, $diameter, $diameter, 0, 90)
    # Bottom Line
    $path.AddLine($x + $width - $radius, $y + $height, $x + $radius, $y + $height)
    # Bottom-Left Arc
    $path.AddArc($x, $y + $height - $diameter, $diameter, $diameter, 90, 90)
    # Left Line
    $path.AddLine($x, $y + $height - $radius, $x, $y + $radius)
    $path.CloseFigure()

    return $path
}

# -------------------------------------------------------------
# 1. Super Zone Badge Frame (Gold Border, Dark Amber Gradient)
# -------------------------------------------------------------
$w = 512
$h = 160
$bmpSuper = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmpSuper)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

$margin = 6.0
$radius = 28.0
$pathSuper = New-RoundedRectanglePath $margin $margin ($w - 2 * $margin) ($h - 2 * $margin) $radius

# Interior Gradient: Rich amber gold at top (#5C3806), dark chocolate brown at bottom (#1F1302)
$topColor = [System.Drawing.Color]::FromArgb(255, 96, 58, 8)
$botColor = [System.Drawing.Color]::FromArgb(255, 30, 18, 2)
$pStart = New-Object System.Drawing.PointF 0, $margin
$pEnd = New-Object System.Drawing.PointF 0, ($h - $margin)
$brushSuper = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @($pStart, $pEnd, $topColor, $botColor)
$g.FillPath($brushSuper, $pathSuper)

# Golden Border: #E0A01E
$goldBorderColor = [System.Drawing.Color]::FromArgb(255, 224, 160, 30)
$penBorder = New-Object System.Drawing.Pen $goldBorderColor, 8.0
$g.DrawPath($penBorder, $pathSuper)

# Inner Golden Accent Line (Subtle highlight inside border)
$innerMargin = $margin + 5.0
$innerRadius = $radius - 4.0
$innerPath = New-RoundedRectanglePath $innerMargin $innerMargin ($w - 2 * $innerMargin) ($h - 2 * $innerMargin) $innerRadius
$innerGlowColor = [System.Drawing.Color]::FromArgb(90, 255, 215, 90)
$penInner = New-Object System.Drawing.Pen $innerGlowColor, 2.0
$g.DrawPath($penInner, $innerPath)

$bmpSuper.Save((Join-Path $demoDir "ui_badge_frame_super.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmpSuper.Dispose()
$g.Dispose()

# -------------------------------------------------------------
# 2. Safe Zone Badge Frame (Green Border, Dark Forest Gradient)
# -------------------------------------------------------------
$bmpSafe = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmpSafe)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

$pathSafe = New-RoundedRectanglePath $margin $margin ($w - 2 * $margin) ($h - 2 * $margin) $radius

# Interior Gradient: Forest green at top (#1A540B), very dark green at bottom (#082003)
$topGreen = [System.Drawing.Color]::FromArgb(255, 26, 84, 11)
$botGreen = [System.Drawing.Color]::FromArgb(255, 8, 32, 3)
$brushSafe = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @($pStart, $pEnd, $topGreen, $botGreen)
$g.FillPath($brushSafe, $pathSafe)

# Green Border: #48B81B
$greenBorderColor = [System.Drawing.Color]::FromArgb(255, 76, 192, 28)
$penGreen = New-Object System.Drawing.Pen $greenBorderColor, 8.0
$g.DrawPath($penGreen, $pathSafe)

# Inner Green Accent Line
$innerGlowGreen = [System.Drawing.Color]::FromArgb(90, 140, 245, 90)
$penInnerGreen = New-Object System.Drawing.Pen $innerGlowGreen, 2.0
$g.DrawPath($penInnerGreen, $innerPath)

$bmpSafe.Save((Join-Path $demoDir "ui_badge_frame_safe.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmpSafe.Dispose()
$g.Dispose()

# -------------------------------------------------------------
# 3. Safe Zone Delta Triangle Icon (Green Greek Delta Δ)
# -------------------------------------------------------------
$size = 256
$bmpDelta = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmpDelta)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

# Outer Triangle Points
$p1 = New-Object System.Drawing.PointF 128, 22
$p2 = New-Object System.Drawing.PointF 232, 228
$p3 = New-Object System.Drawing.PointF 24, 228

# Inner Triangle Points (Cutout)
$ip1 = New-Object System.Drawing.PointF 128, 98
$ip2 = New-Object System.Drawing.PointF 186, 202
$ip3 = New-Object System.Drawing.PointF 70, 202

$deltaPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$deltaPath.AddPolygon([System.Drawing.PointF[]]@($p1, $p2, $p3))
$deltaPath.AddPolygon([System.Drawing.PointF[]]@($ip1, $ip3, $ip2)) # Reverse order to create hole

# Gradient: Bright lime green at apex (#8AF436) to deep rich green at base (#268A10)
$topDelta = [System.Drawing.Color]::FromArgb(255, 142, 246, 56)
$botDelta = [System.Drawing.Color]::FromArgb(255, 36, 138, 16)
$dpStart = New-Object System.Drawing.PointF 0, 22
$dpEnd = New-Object System.Drawing.PointF 0, 228
$brushDelta = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @($dpStart, $dpEnd, $topDelta, $botDelta)
$g.FillPath($brushDelta, $deltaPath)

# Crisp edge strokes
$penDeltaEdge = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(220, 190, 255, 120)), 3.0
$g.DrawPolygon($penDeltaEdge, [System.Drawing.PointF[]]@($p1, $p2, $p3))
$g.DrawPolygon($penDeltaEdge, [System.Drawing.PointF[]]@($ip1, $ip2, $ip3))

$bmpDelta.Save((Join-Path $demoDir "ui_icon_badge_delta.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmpDelta.Dispose()
$g.Dispose()

# -------------------------------------------------------------
# 4. Super Zone Sword Icon (Truncated Claymore matching reference)
# -------------------------------------------------------------
$bmpSword = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmpSword)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

# Rotate canvas by -20 degrees around center (128, 128)
$g.TranslateTransform(128, 128)
$g.RotateTransform(-20)

# 4A. Pommel: 3-pointed flared crown shape (Y = -106 to -90)
$pommelPoints = @(
    (New-Object System.Drawing.PointF -8, -102),
    (New-Object System.Drawing.PointF -4, -98),
    (New-Object System.Drawing.PointF 0, -106),
    (New-Object System.Drawing.PointF 4, -98),
    (New-Object System.Drawing.PointF 8, -102),
    (New-Object System.Drawing.PointF 6, -90),
    (New-Object System.Drawing.PointF -6, -90)
)
$pommelStart = New-Object System.Drawing.PointF -8, -106
$pommelEnd = New-Object System.Drawing.PointF 8, -90
$brushPommel = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @($pommelStart, $pommelEnd, [System.Drawing.Color]::FromArgb(255, 230, 235, 245), [System.Drawing.Color]::FromArgb(255, 90, 100, 115))
$g.FillPolygon($brushPommel, [System.Drawing.PointF[]]$pommelPoints)
$penPommel = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 60, 65, 75)), 1.5
$g.DrawPolygon($penPommel, [System.Drawing.PointF[]]$pommelPoints)

# 4B. Grip: Y = -90 to -36, width 11
$gripStart = New-Object System.Drawing.PointF -6, 0
$gripEnd = New-Object System.Drawing.PointF 6, 0
$gripBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @($gripStart, $gripEnd, [System.Drawing.Color]::FromArgb(255, 45, 50, 58), [System.Drawing.Color]::FromArgb(255, 110, 115, 125))
$g.FillRectangle($gripBrush, -6, -90, 12, 54)

# Segmented grip metal rings every 7px
$penRing = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 190, 200, 215)), 1.5
$penRingDark = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 30, 35, 42)), 1.0
for ($y = -84; $y -lt -36; $y += 7) {
    $g.DrawLine($penRing, -6, $y, 6, $y)
    $g.DrawLine($penRingDark, -6, ($y + 1), 6, ($y + 1))
}
$g.DrawRectangle($penPommel, -6, -90, 12, 54)

# 4C. Crossguard: Y = -36 to -22, width 72 with upturned tips
$guardPoints = @(
    (New-Object System.Drawing.PointF -36, -30),
    (New-Object System.Drawing.PointF -32, -34),
    (New-Object System.Drawing.PointF -10, -32),
    (New-Object System.Drawing.PointF 0, -35),
    (New-Object System.Drawing.PointF 10, -32),
    (New-Object System.Drawing.PointF 32, -34),
    (New-Object System.Drawing.PointF 36, -30),
    (New-Object System.Drawing.PointF 28, -22),
    (New-Object System.Drawing.PointF 8, -24),
    (New-Object System.Drawing.PointF 0, -22),
    (New-Object System.Drawing.PointF -8, -24),
    (New-Object System.Drawing.PointF -28, -22)
)
$guardStart = New-Object System.Drawing.PointF 0, -36
$guardEnd = New-Object System.Drawing.PointF 0, -22
$guardBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @($guardStart, $guardEnd, [System.Drawing.Color]::FromArgb(255, 240, 245, 252), [System.Drawing.Color]::FromArgb(255, 110, 120, 135))
$g.FillPolygon($guardBrush, [System.Drawing.PointF[]]$guardPoints)
$penGuard = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 50, 55, 65)), 1.5
$g.DrawPolygon($penGuard, [System.Drawing.PointF[]]$guardPoints)

# Center diamond boss on guard with gold center
$bossPoints = @(
    (New-Object System.Drawing.PointF 0, -34),
    (New-Object System.Drawing.PointF 6, -28),
    (New-Object System.Drawing.PointF 0, -22),
    (New-Object System.Drawing.PointF -6, -28)
)
$bossBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @((New-Object System.Drawing.PointF 0, -34), (New-Object System.Drawing.PointF 0, -22), [System.Drawing.Color]::FromArgb(255, 255, 220, 90), [System.Drawing.Color]::FromArgb(255, 180, 120, 25))
$g.FillPolygon($bossBrush, [System.Drawing.PointF[]]$bossPoints)
$g.DrawPolygon($penGuard, [System.Drawing.PointF[]]$bossPoints)

# 4D. Truncated Upper Blade: Y = -22 to +46 (NO TIP, truncated cleanly!)
$bladeLeft = @(
    (New-Object System.Drawing.PointF -11, -22),
    (New-Object System.Drawing.PointF 0, -22),
    (New-Object System.Drawing.PointF 0, 46),
    (New-Object System.Drawing.PointF -9, 46)
)
$bLeftStart = New-Object System.Drawing.PointF -11, 0
$bLeftEnd = New-Object System.Drawing.PointF 0, 0
$bLeftBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @($bLeftStart, $bLeftEnd, [System.Drawing.Color]::FromArgb(255, 245, 250, 255), [System.Drawing.Color]::FromArgb(255, 185, 195, 210))
$g.FillPolygon($bLeftBrush, [System.Drawing.PointF[]]$bladeLeft)

$bladeRight = @(
    (New-Object System.Drawing.PointF 0, -22),
    (New-Object System.Drawing.PointF 11, -22),
    (New-Object System.Drawing.PointF 9, 46),
    (New-Object System.Drawing.PointF 0, 46)
)
$bRightStart = New-Object System.Drawing.PointF 0, 0
$bRightEnd = New-Object System.Drawing.PointF 11, 0
$bRightBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush -ArgumentList @($bRightStart, $bRightEnd, [System.Drawing.Color]::FromArgb(255, 135, 145, 160), [System.Drawing.Color]::FromArgb(255, 75, 85, 100))
$g.FillPolygon($bRightBrush, [System.Drawing.PointF[]]$bladeRight)

# Central fuller groove with gold inlay line
$penFullerDark = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(200, 40, 45, 55)), 2.5
$g.DrawLine($penFullerDark, 0, -22, 0, 46)
$penFullerGold = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 230, 180, 40)), 1.2
$g.DrawLine($penFullerGold, 0, -22, 0, 46)

# Blade outline (truncated flat at y = 46)
$bladeOutline = @(
    (New-Object System.Drawing.PointF -11, -22),
    (New-Object System.Drawing.PointF -9, 46),
    (New-Object System.Drawing.PointF 9, 46),
    (New-Object System.Drawing.PointF 11, -22)
)
$penBlade = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 210, 220, 235)), 1.2
$g.DrawPolygon($penBlade, [System.Drawing.PointF[]]$bladeOutline)

$bmpSword.Save((Join-Path $demoDir "ui_icon_badge_sword.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmpSword.Dispose()
$g.Dispose()

Write-Host "All 4 badge assets successfully generated!"
