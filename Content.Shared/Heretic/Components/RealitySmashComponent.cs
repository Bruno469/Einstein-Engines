using Robust.Shared.GameStates;

namespace Content.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedRealitySmashSystem))]
public sealed partial class RealitySmashComponent : Component
{
    [DataField("isUsed"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool IsUsed = false;
}