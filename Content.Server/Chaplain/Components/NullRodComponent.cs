using Content.Server.Chaplain;
using Content.Shared.Chaplain.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Server.Chaplain.Components;

/// <summary>
/// This component stores the possible contents of the backpack,
/// which can be selected via the interface.
/// </summary>
[RegisterComponent, Access(typeof(NullRodUpdateSystem))]
public sealed partial class NullRodComponent : Component
{
    /// <summary>
    /// List of sets available for selection
    /// </summary>
    [DataField]
    public List<ProtoId<ChaplainSetPrototype>> PossibleSets = new();

    [DataField]
    public List<int> SelectedSets = new();

    [DataField]
    public SoundSpecifier ApproveSound = new SoundPathSpecifier("/Audio/Effects/rustle1.ogg");

    [DataField("tryMessage")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string TryMessage = "prayer-popup-notify-try-Update";

    /// <summary>
    /// Used in window title and context menu
    /// </summary>
    [DataField("verb")]
    [ViewVariables(VVAccess.ReadOnly)]
    public string Verb = "activate-verb";

    /// <summary>
    /// Context menu image
    /// </summary>
    [DataField("verbImage")]
    [ViewVariables(VVAccess.ReadOnly)]
    public SpriteSpecifier? VerbImage = new SpriteSpecifier.Texture(new ("/Textures/Interface/pray.svg.png"));
}
