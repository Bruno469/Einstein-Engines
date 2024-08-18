using JetBrains.Annotations;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;

namespace Content.Server.Atmos.Reactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class ElectrolysisReaction : IGasReactionEffect
    {
        public float RequiredEnergy { get; }
        public IDictionary<Gas, float> GasesProduced { get; }
        public IDictionary<Gas, float> MinimumRequirements { get; }

        public ElectrolysisReaction(float requiredEnergy, IDictionary<Gas, float> gasesProduced, IDictionary<Gas, float> minimumRequirements)
        {
            RequiredEnergy = requiredEnergy;
            GasesProduced = gasesProduced;
            MinimumRequirements = minimumRequirements;
        }

        public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
        {
            var temperature = mixture.Temperature;
            var location = holder as TileAtmosphere;

            foreach (var gas in MinimumRequirements)
            {
                var currentMoles = mixture.GetMoles(gas.Key);
                var requiredMoles = gas.Value;
                if (currentMoles > requiredMoles)
                {
                    mixture.SetMoles(gas.Key, currentMoles - requiredMoles);
                }
                else
                {
                    mixture.SetMoles(gas.Key, 0);
                }
            }

            foreach (var gas in GasesProduced)
            {
                mixture.AdjustMoles(gas.Key, gas.Value);
            }

            var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
            if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
            {
                mixture.Temperature = temperature;
            }

            return ReactionResult.Reacting;
        }
    }
}
