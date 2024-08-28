using Content.Shared.Heretic.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.Heretic.UI;

[UsedImplicitly]
public sealed class HereticResearchBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private HereticResearchMenu? _hereticMenu;

    public HereticResearchBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        var owner = Owner;

        _hereticMenu = new HereticResearchMenu(owner);

        _hereticMenu.OnHereticCardPressed += id =>
        {
            SendMessage(new UnlockHereticResearchMessage(id));
        };

        _hereticMenu.OnClose += Close;

        _hereticMenu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not HereticResearchBoundInterfaceState castState)
            return;
        _hereticMenu?.UpdatePanels(castState);
        _hereticMenu?.UpdateInformationPanel(castState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _hereticMenu?.Dispose();
    }
}
