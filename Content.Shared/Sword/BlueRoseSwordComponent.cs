using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Sword
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class BlueRoseSwordComponent : Component
    {
        [DataField("requiresBibleUser")]
        public bool RequiresBibleUser = true;

        public bool AlreadyReinforce = false;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("prototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? Prototype;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("radius")]
        public int Radius;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("points")]
        public int Points;

        [DataField("reinforceAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ReinforceAction = "ActionReinforceArmament";

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("reinforceActionEntity")]
        public EntityUid? ReinforceActionEntity;
    }
}
