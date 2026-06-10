using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace ShadowUI;

/// <summary>Chart data point: X-axis label and numeric value.</summary>
public record ChartPoint(string Label, double Value);

/// <summary>Vertical bar chart. Renders via DrawingContext; AOT-compatible.</summary>
public class BarChart : Control
{
#pragma warning disable CS1591
    public static readonly StyledProperty<IEnumerable<ChartPoint>?> PointsProperty =
        AvaloniaProperty.Register<BarChart, IEnumerable<ChartPoint>?>(nameof(Points));

    public static readonly StyledProperty<string> BarBrushKeyProperty =
        AvaloniaProperty.Register<BarChart, string>(nameof(BarBrushKey), "ShadowChart1Brush");
#pragma warning restore CS1591

    /// <summary>Data points to display as bars.</summary>
    public IEnumerable<ChartPoint>? Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    /// <summary>Resource key for the bar brush (defaults to ShadowChart1Brush).</summary>
    public string BarBrushKey
    {
        get => GetValue(BarBrushKeyProperty);
        set => SetValue(BarBrushKeyProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(BarChart);

    private List<Rect> _barRects = new();

    static BarChart()
    {
        PointsProperty.Changed.AddClassHandler<BarChart>((c, _) => c.InvalidateVisual());
        BarBrushKeyProperty.Changed.AddClassHandler<BarChart>((c, _) => c.InvalidateVisual());
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        var idx = _barRects.FindIndex(r => r.Contains(pos));
        var pts = Points?.ToList();
        if (idx >= 0 && pts is not null)
        {
            var pt = pts.ElementAtOrDefault(idx);
            if (pt is not null)
                ToolTip.SetTip(this, $"{pt.Label}: {pt.Value:G4}");
            else
                ToolTip.SetTip(this, null);
        }
        else
        {
            ToolTip.SetTip(this, null);
        }
        base.OnPointerMoved(e);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var pts = Points?.ToList();
        if (pts is null || pts.Count == 0) return;

        IBrush? brush = null;
        if (this.TryFindResource(BarBrushKey, ActualThemeVariant, out var res) && res is IBrush b)
            brush = b;
        brush ??= Brushes.SteelBlue;

        double w = Bounds.Width, h = Bounds.Height;
        const double padL = 40, padB = 24, padR = 8, padT = 8;
        double chartW = w - padL - padR;
        double chartH = h - padT - padB;

        if (chartW <= 0 || chartH <= 0) return;

        double maxVal = pts.Max(p => p.Value);
        // T-07-03-03: guard for NaN/Inf and zero values
        if (double.IsNaN(maxVal) || double.IsInfinity(maxVal) || maxVal <= 0)
            maxVal = 1;

        double barWidth = chartW / pts.Count * 0.6;
        double gap = chartW / pts.Count;

        _barRects.Clear();

        for (int i = 0; i < pts.Count; i++)
        {
            var pt = pts[i];
            double val = double.IsNaN(pt.Value) || double.IsInfinity(pt.Value) ? 0 : pt.Value;
            double barH = val / maxVal * chartH;
            double x = padL + i * gap + (gap - barWidth) / 2;
            double y = padT + chartH - barH;
            var rect = new Rect(x, y, barWidth, barH);
            context.DrawRectangle(brush, null, rect);
            _barRects.Add(rect);
        }

        IBrush labelBrush = this.TryFindResource("ShadowMutedForegroundBrush", ActualThemeVariant, out var lr) && lr is IBrush lb
            ? lb
            : Brushes.Gray;

        for (int i = 0; i < pts.Count; i++)
        {
            var pt = pts[i];
            var ft = new FormattedText(
                pt.Label,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                10,
                labelBrush);
            context.DrawText(ft, new Point(padL + i * gap + gap / 2 - ft.Width / 2, padT + chartH + 4));
        }

        context.DrawLine(new Pen(labelBrush, 1), new Point(padL, padT), new Point(padL, padT + chartH));

        var ftMax = new FormattedText(
            $"{maxVal:G3}",
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            10,
            labelBrush);
        context.DrawText(ftMax, new Point(2, padT));
    }
}

/// <summary>Line chart. Renders via DrawingContext; AOT-compatible.</summary>
public class LineChart : Control
{
#pragma warning disable CS1591
    public static readonly StyledProperty<IEnumerable<ChartPoint>?> PointsProperty =
        AvaloniaProperty.Register<LineChart, IEnumerable<ChartPoint>?>(nameof(Points));

    public static readonly StyledProperty<string> LineBrushKeyProperty =
        AvaloniaProperty.Register<LineChart, string>(nameof(LineBrushKey), "ShadowChart2Brush");
#pragma warning restore CS1591

    /// <summary>Data points to display as a line.</summary>
    public IEnumerable<ChartPoint>? Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    /// <summary>Resource key for the line brush (defaults to ShadowChart2Brush).</summary>
    public string LineBrushKey
    {
        get => GetValue(LineBrushKeyProperty);
        set => SetValue(LineBrushKeyProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(LineChart);

    private List<Point> _pointCoords = new();

    static LineChart()
    {
        PointsProperty.Changed.AddClassHandler<LineChart>((c, _) => c.InvalidateVisual());
        LineBrushKeyProperty.Changed.AddClassHandler<LineChart>((c, _) => c.InvalidateVisual());
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var pos = e.GetPosition(this);

        int closestIdx = -1;
        double minDist = double.MaxValue;
        for (int i = 0; i < _pointCoords.Count; i++)
        {
            var p = _pointCoords[i];
            double dx = p.X - pos.X;
            double dy = p.Y - pos.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < minDist)
            {
                minDist = dist;
                closestIdx = i;
            }
        }

        var pts = Points?.ToList();
        if (closestIdx >= 0 && minDist <= 20 && pts is not null)
        {
            var pt = pts.ElementAtOrDefault(closestIdx);
            if (pt is not null)
                ToolTip.SetTip(this, $"{pt.Label}: {pt.Value:G4}");
            else
                ToolTip.SetTip(this, null);
        }
        else
        {
            ToolTip.SetTip(this, null);
        }
        base.OnPointerMoved(e);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var pts = Points?.ToList();
        if (pts is null || pts.Count == 0) return;

        IBrush? brush = null;
        if (this.TryFindResource(LineBrushKey, ActualThemeVariant, out var res) && res is IBrush b)
            brush = b;
        brush ??= Brushes.CornflowerBlue;

        double w = Bounds.Width, h = Bounds.Height;
        const double padL = 40, padB = 24, padR = 8, padT = 8;
        double chartW = w - padL - padR;
        double chartH = h - padT - padB;

        if (chartW <= 0 || chartH <= 0) return;

        double maxVal = pts.Max(p => p.Value);
        // T-07-03-03: guard for NaN/Inf and zero values
        if (double.IsNaN(maxVal) || double.IsInfinity(maxVal) || maxVal <= 0)
            maxVal = 1;

        double gap = chartW / pts.Count;

        _pointCoords.Clear();

        for (int i = 0; i < pts.Count; i++)
        {
            var pt = pts[i];
            double val = double.IsNaN(pt.Value) || double.IsInfinity(pt.Value) ? 0 : pt.Value;
            double x = padL + i * gap + gap / 2;
            double y = padT + chartH - val / maxVal * chartH;
            _pointCoords.Add(new Point(x, y));
        }

        for (int i = 1; i < _pointCoords.Count; i++)
            context.DrawLine(new Pen(brush, 2), _pointCoords[i - 1], _pointCoords[i]);

        foreach (var p in _pointCoords)
            context.DrawEllipse(brush, null, p, 4, 4);

        IBrush labelBrush = this.TryFindResource("ShadowMutedForegroundBrush", ActualThemeVariant, out var lr) && lr is IBrush lb
            ? lb
            : Brushes.Gray;

        for (int i = 0; i < pts.Count; i++)
        {
            var pt = pts[i];
            var ft = new FormattedText(
                pt.Label,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                10,
                labelBrush);
            context.DrawText(ft, new Point(padL + i * gap + gap / 2 - ft.Width / 2, padT + chartH + 4));
        }

        context.DrawLine(new Pen(labelBrush, 1), new Point(padL, padT), new Point(padL, padT + chartH));

        var ftMax = new FormattedText(
            $"{maxVal:G3}",
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            10,
            labelBrush);
        context.DrawText(ftMax, new Point(2, padT));
    }
}
