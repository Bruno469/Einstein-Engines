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
        /// The efficience of the reaction is calculed based in temperature (more temperature = more efficiency)
        /// </summary>
        [DataField("efficiency")]
        public float Efficiency { get; private set; }

        /// <summary>
        /// minimumRequirements to make eletrolise
        /// </summary>
        [DataField("minimumRequirements")]
        public float[] MinimumRequirements { get; private set; } = new float[Atmospherics.TotalNumberOfGases];

        [DataField("Produces")]
        public float[] Produces { get; private set; } = new float[Atmospherics.TotalNumberOfGases];
    }
}
