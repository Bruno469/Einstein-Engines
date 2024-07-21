using Content.Shared.PsycUpdate;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.PsycUpdate;

[UsedImplicitly]
public sealed class UpdateCristalBoundUserInterface : BoundUserInterface
{
    private UpdateCristalMenu? _window;

    public UpdateCristalBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = new UpdateCristalMenu(this);
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        if (_window != null)
            _window.OnClose -= Close;

        _window?.Dispose();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not UpdateCristalBoundUserInterfaceState current)
            return;

        _window?.UpdateState(current);
    }

    public void SendChangeSelected(int setNumber)
    {
        SendMessage(new UpdateCristalChangeSetMessage(setNumber));
    }

    public void SendApprove()
    {
        SendMessage(new UpdateCristalApproveMessage());
    }
}
