using System;

namespace ShadowUI;

/// <summary>Confirmation dialog (shadcn AlertDialog analogue): like <see cref="Dialog"/>,
/// but does NOT close on outside click or Esc — only via explicit buttons. Uses the Dialog theme.</summary>
public class AlertDialog : Dialog
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Dialog);

    /// <summary>Creates an AlertDialog with overlay/Esc closing disabled and no close button.</summary>
    public AlertDialog()
    {
        CloseOnClickOutside = false;
        CloseOnEscape = false;
        ShowCloseButton = false;
    }
}
