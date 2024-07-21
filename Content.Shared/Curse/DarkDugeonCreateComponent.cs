using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;

namespace Content.Shared.Curse;

[RegisterComponent, NetworkedComponent]
public sealed partial class DarkDugeonCreateComponent : Component
{
    /// <summary>
    /// If bible users are only allowed to use this prayable entity
    /// </summary>
    [DataField("bibleUserOnly")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool BibleUserOnly = true;

    [DataField("tryMessage")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string TryMessage = "prayer-popup-notify-try-tp";

    [DataField("notificationTeleport")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string NotificationPrefix = "prayer-chat-notify-Teleport";

    /// <summary>
    /// Used in window title and context menu
    /// </summary>
    [DataField("verb")]
    [ViewVariables(VVAccess.ReadOnly)]
    public string Verb = "observe-verb";

    /// <summary>
    /// Context menu image
    /// </summary>
    [DataField("verbImage")]
    [ViewVariables(VVAccess.ReadOnly)]
    public SpriteSpecifier? VerbImage = new SpriteSpecifier.Texture(new ("/Textures/Interface/pray.svg.png"));
}
