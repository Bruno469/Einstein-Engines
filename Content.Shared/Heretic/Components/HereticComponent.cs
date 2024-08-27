using Content.Shared.Antag;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticComponent : Component
{
    [DataField]
    public SoundSpecifier HereticStartSound = new SoundPathSpecifier("/Audio/Ambience/Antag/heretic_start.ogg");

    public override bool SessionSpecific => true;

    [DataField("MansusHandToggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string MansusHandToggleAction = "ActionToggleMansusHand";
    [DataField("MansusHandActionEntity")]
    public EntityUid? MansusHandActionEntity;

    [DataField("MansusHandProtoId")]
    public EntProtoId MansusHandProtoId = "MansusHand";

    //Find hidden influences and sacrifice crew members to gain magical
    //powers and ascend as one of several paths.

    //Forgotten, devoured, gutted. Humanity has forgotten the eldritch forces
    //of decay, but the mansus veil has weakened. We will make them taste fear
    //again...

    [NetSerializable, Serializable]
    public enum HereticResearchUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class UnlockHereticResearchMessage : BoundUserInterfaceMessage
    {
        public string Id;

        public UnlockHereticResearchMessage(string id)
        {
            Id = id;
        }
    }

    [Serializable, NetSerializable]
    public sealed class HereticResearchBoundInterfaceState : BoundUserInterfaceState
    {
        public int Points;
        public HereticResearchBoundInterfaceState(int points)
        {
            Points = points;
        }
    }
}