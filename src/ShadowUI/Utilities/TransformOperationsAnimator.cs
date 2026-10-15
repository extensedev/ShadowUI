using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace ShadowUI;

/// <summary>
/// Keyframe animator for <c>RenderTransform</c> values written as transform operations (<c>rotate(360deg)</c>).
/// Animating the visual's own <c>RenderTransform</c> keeps Avalonia's pause-when-hidden and stop-on-detach,
/// which are lost when a <c>RotateTransform.Angle</c> is animated.
/// </summary>
internal sealed class TransformOperationsAnimator : InterpolatingAnimator<ITransform>
{
    public override ITransform Interpolate(double progress, ITransform oldValue, ITransform newValue) =>
        TransformOperations.Interpolate((TransformOperations)oldValue, (TransformOperations)newValue, progress);
}
