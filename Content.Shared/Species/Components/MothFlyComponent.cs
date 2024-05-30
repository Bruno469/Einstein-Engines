using Content.Shared.Mobs;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.Species.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MothFlyComponent : Component
{
    /// <summary>
    /// The action to use.
    /// </summary>
    [DataField("actionPrototype", required: true)]
    public EntProtoId ActionPrototype;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// What mob states the action will appear in
    /// </summary>
    [DataField("allowedStates"), ViewVariables(VVAccess.ReadWrite)]
    public List<MobState> AllowedStates = new();

    /// <summary>
    /// The text that appears when attempting to split.
    /// </summary>
    [DataField("popupText")]
    public string PopupText = "moth-fly-action-use";
    [DataField("popupText")]
    public string StopPopupText = "moth-fly-stop-action-use";
}
