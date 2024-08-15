using Content.Shared.Chaplain;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.Chaplain;

[UsedImplicitly]
public sealed class NullRodBoundUserInterface : BoundUserInterface
{
    private NullRodMenu? _window;

    public NullRodBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = new NullRodMenu(this);
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

        if (state is not NullRodBoundUserInterfaceState current)
            return;

        _window?.UpdateState(current);
    }

    public void SendChangeSelected(int setNumber)
    {
        SendMessage(new NullRodChangeSetMessage(setNumber));
    }

    public void SendApprove()
    {
        SendMessage(new NullRodApproveMessage());
    }
}
