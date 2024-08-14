using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Atmos.Prototypes
{
    [Prototype("GasEletrolyzer")]
    public sealed partial class EletrolyzerGasPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        /// <summary>
        ///     Specific heat for gas.
        /// </summary>
        [DataField("efficiency")]
        public float Efficiency { get; private set; }

        /// <summary>
        /// The reagent that this gas will turn into when inhaled.
        /// </summary>
        [DataField("gas", customTypeSerializer:typeof(PrototypeIdSerializer<GasPrototype>))]
        public string? gas { get; private set; } = default!;

        [DataField("gasTo", customTypeSerializer:typeof(PrototypeIdSerializer<GasPrototype>))]
        public string? GasTo { get; private set; } = default!;
    }
}
