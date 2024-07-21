using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Curse
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class SpawnOnColliderComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("Prototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? Prototype;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("radius")]
        public int radius;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("points")]
        public int points;
    }
}
