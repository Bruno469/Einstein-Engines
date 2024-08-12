using Robust.Shared.GameStates;
using Content.Shared.Heretic.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Map;

namespace Content.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CanDrawnRuneComponent : Component
{
    public bool IsUsed = false;
    [DataField("doAfterDuration"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(13.4);
    [DataField("EffectProtoId")]
    public EntProtoId EffectProtoId = "RuneDrawing";

    public EntityCoordinates LocationToDrawn;
    public EntityUid? ActiveEffect;
}

[Serializable, NetSerializable]
public sealed partial class StartDrawnDoAfterEvent : SimpleDoAfterEvent
{

}
