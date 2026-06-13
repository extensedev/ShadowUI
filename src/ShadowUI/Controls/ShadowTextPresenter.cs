using Avalonia;
using Avalonia.Controls.Presenters;

namespace ShadowUI;

/// <summary>
/// <see cref="TextPresenter"/> that re-applies the selection foreground brush when the selection changes.
/// Works around an Avalonia 12.0.4 bug: selection changes call the cache-preserving layout invalidation,
/// so the shaped text runs keep their original foreground and <c>SelectionForegroundBrush</c> never paints.
/// Forcing a full <see cref="TextPresenter.InvalidateTextLayout"/> clears the run cache so the selected
/// text re-colors. Used by the ShadowUI TextBox template; inherited by every field control built on it.
/// </summary>
public class ShadowTextPresenter : TextPresenter
{
    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectionStartProperty || change.Property == SelectionEndProperty)
        {
            InvalidateTextLayout();
        }
    }
}
