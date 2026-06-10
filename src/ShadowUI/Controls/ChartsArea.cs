using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace ShadowUI;

/// <summary>Area chart: a line with a fill underneath (shadcn Area Chart). Rendered via DrawingContext; AOT-compatible.</summary>
public class AreaChart : Control
{
#pragma warning disable CS1591
    public static readonly StyledProperty<IEnumerable<ChartPoint>?> PointsProperty =
        AvaloniaProperty.Register<AreaChart, IEnumerable<ChartPoint>?>(nameof(Points));

    public static readonly StyledProperty<string> AreaBrushKeyProperty =
        AvaloniaProperty.Register<AreaChart, string>(nameof(AreaBrushKey), "ShadowChart1Brush");
#pragma warning restore CS1591

    /// <summary>Data points.</summary>
    public IEnumerable<ChartPoint>? Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    /// <summary>Resource key of the line/fill brush (ShadowChart1Brush by default).</summary>
    public string AreaBrushKey
    {
        get => GetValue(AreaBrushKeyProperty);
        set => SetValue(AreaBrushKeyProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(AreaChart);

    private readonly List<Point> _pointCoords = new();

    static AreaChart()
    {
        PointsProperty.Changed.AddClassHandler<AreaChart>((c, _) => c.InvalidateVisual());
        AreaBrushKeyProperty.Changed.AddClassHandler<AreaChart>((c, _) => c.InvalidateVisual());
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        int closestIdx = -1;
        double minDist = double.MaxValue;
        for (int i = 0; i < _pointCoords.Count; i++)
        {
            double dx = _pointCoords[i].X - pos.X;
            double dy = _pointCoords[i].Y - pos.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < minDist) { minDist = dist; closestIdx = i; }
        }

        var pts = Points?.ToList();
        if (closestIdx >= 0 && minDist <= 20 && pts is not null && pts.ElementAtOrDefault(closestIdx) is { } pt)
            ToolTip.SetTip(this, $"{pt.Label}: {pt.Value:G4}");
        else
            ToolTip.SetTip(this, null);
        base.OnPointerMoved(e);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var pts = Points?.ToList();
        if (pts is null || pts.Count == 0) return;

        IBrush? brush = null;
        if (this.TryFindResource(AreaBrushKey, ActualThemeVariant, out var res) && res is IBrush b)
            brush = b;
        brush ??= Brushes.CornflowerBlue;

        double w = Bounds.Width, h = Bounds.Height;
        const double padL = 40, padB = 24, padR = 8, padT = 8;
        double chartW = w - padL - padR;
        double chartH = h - padT - padB;
        if (chartW <= 0 || chartH <= 0) return;

        double maxVal = pts.Max(p => p.Value);
        if (double.IsNaN(maxVal) || double.IsInfinity(maxVal) || maxVal <= 0)
            maxVal = 1;

        double gap = chartW / pts.Count;
        double baseline = padT + chartH;

        _pointCoords.Clear();
        for (int i = 0; i < pts.Count; i++)
        {
            double val = double.IsNaN(pts[i].Value) || double.IsInfinity(pts[i].Value) ? 0 : pts[i].Value;
            _pointCoords.Add(new Point(padL + i * gap + gap / 2, padT + chartH - val / maxVal * chartH));
        }

        // area fill (25% opacity, as in the shadcn area chart)
        var fill = new StreamGeometry();
        using (var ctx = fill.Open())
        {
            ctx.BeginFigure(new Point(_pointCoords[0].X, baseline), isFilled: true);
            foreach (var p in _pointCoords)
                ctx.LineTo(p);
            ctx.LineTo(new Point(_pointCoords[^1].X, baseline));
            ctx.EndFigure(true);
        }
        using (context.PushOpacity(0.25))
            context.DrawGeometry(brush, null, fill);

        for (int i = 1; i < _pointCoords.Count; i++)
            context.DrawLine(new Pen(brush, 2), _pointCoords[i - 1], _pointCoords[i]);

        IBrush labelBrush = this.TryFindResource("ShadowMutedForegroundBrush", ActualThemeVariant, out var lr) && lr is IBrush lb
            ? lb
            : Brushes.Gray;

        for (int i = 0; i < pts.Count; i++)
        {
            var ft = new FormattedText(
                pts[i].Label,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                10,
                labelBrush);
            context.DrawText(ft, new Point(padL + i * gap + gap / 2 - ft.Width / 2, baseline + 4));
        }

        context.DrawLine(new Pen(labelBrush, 1), new Point(padL, padT), new Point(padL, baseline));

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

/// <summary>Pie/donut chart (shadcn Pie Chart). Cycles through the ShadowChart1..5Brush brushes. AOT-compatible.</summary>
public class PieChart : Control
{
#pragma warning disable CS1591
    public static readonly StyledProperty<IEnumerable<ChartPoint>?> PointsProperty =
        AvaloniaProperty.Register<PieChart, IEnumerable<ChartPoint>?>(nameof(Points));

    public static readonly StyledProperty<double> InnerRadiusRatioProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(InnerRadiusRatio), 0.6);
#pragma warning restore CS1591

    /// <summary>Data points (Label + Value); each one is a sector.</summary>
    public IEnumerable<ChartPoint>? Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    /// <summary>Inner radius fraction (0 = solid pie, 0.6 = donut, the default).</summary>
    public double InnerRadiusRatio
    {
        get => GetValue(InnerRadiusRatioProperty);
        set => SetValue(InnerRadiusRatioProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PieChart);

    private readonly List<(double start, double sweep)> _slices = new();

    static PieChart()
    {
        PointsProperty.Changed.AddClassHandler<PieChart>((c, _) => c.InvalidateVisual());
        InnerRadiusRatioProperty.Changed.AddClassHandler<PieChart>((c, _) => c.InvalidateVisual());
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        double radius = Math.Min(Bounds.Width, Bounds.Height) / 2 - 4;
        double dx = pos.X - center.X, dy = pos.Y - center.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        var pts = Points?.ToList();
        if (pts is not null && dist <= radius && dist >= radius * Math.Clamp(InnerRadiusRatio, 0, 0.95))
        {
            // angle from 12 o'clock, clockwise
            double angle = (Math.Atan2(dx, -dy) + Math.Tau) % Math.Tau;
            for (int i = 0; i < _slices.Count && i < pts.Count; i++)
            {
                var (start, sweep) = _slices[i];
                if (angle >= start && angle < start + sweep)
                {
                    ToolTip.SetTip(this, $"{pts[i].Label}: {pts[i].Value:G4}");
                    base.OnPointerMoved(e);
                    return;
                }
            }
        }
        ToolTip.SetTip(this, null);
        base.OnPointerMoved(e);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var pts = Points?.Where(p => !double.IsNaN(p.Value) && !double.IsInfinity(p.Value) && p.Value > 0).ToList();
        if (pts is null || pts.Count == 0) return;

        double total = pts.Sum(p => p.Value);
        if (total <= 0) return;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        double radius = Math.Min(Bounds.Width, Bounds.Height) / 2 - 4;
        if (radius <= 0) return;
        double inner = radius * Math.Clamp(InnerRadiusRatio, 0, 0.95);

        _slices.Clear();
        double angle = 0; // radians from 12 o'clock, clockwise
        const double gapAngle = 0.03; // gap between sectors as in shadcn

        for (int i = 0; i < pts.Count; i++)
        {
            double sweep = pts[i].Value / total * Math.Tau;
            _slices.Add((angle, sweep));

            IBrush brush = this.TryFindResource($"ShadowChart{i % 5 + 1}Brush", ActualThemeVariant, out var res) && res is IBrush b
                ? b
                : Brushes.SteelBlue;

            double a0 = angle + (pts.Count > 1 ? gapAngle / 2 : 0);
            double a1 = angle + sweep - (pts.Count > 1 ? gapAngle / 2 : 0);
            if (a1 <= a0) { angle += sweep; continue; }

            var geo = new StreamGeometry();
            using (var ctx = geo.Open())
            {
                Point PtOn(double a, double r) => new(center.X + Math.Sin(a) * r, center.Y - Math.Cos(a) * r);
                bool large = a1 - a0 > Math.PI;
                ctx.BeginFigure(PtOn(a0, radius), isFilled: true);
                ctx.ArcTo(PtOn(a1, radius), new Size(radius, radius), 0, large, SweepDirection.Clockwise);
                if (inner > 0)
                {
                    ctx.LineTo(PtOn(a1, inner));
                    ctx.ArcTo(PtOn(a0, inner), new Size(inner, inner), 0, large, SweepDirection.CounterClockwise);
                }
                else
                {
                    ctx.LineTo(center);
                }
                ctx.EndFigure(true);
            }
            context.DrawGeometry(brush, null, geo);

            angle += sweep;
        }
    }
}
