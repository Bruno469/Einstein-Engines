using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MansusHandComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("sparks");

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan StunTime = TimeSpan.FromSeconds(8);
    [DataField("speech")]
    public string Speech { get; private set; }

    public EntityUid Actor;
}