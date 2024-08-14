using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class HydrogenFireReaction : IGasReactionEffect
    {
        public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
        {
            var energyReleased = 0f;
            var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
            var temperature = mixture.Temperature;
            var location = holder as TileAtmosphere;
            mixture.ReactionResults[GasReaction.Fire] = 0f;
            var burnedFuel = 0f;
            var initialHydrogen = mixture.GetMoles(Gas.Hydrogen);

            if (mixture.GetMoles(Gas.Oxygen) < initialHydrogen ||
                Atmospherics.MinimumHydrogenOxyburnEnergy > (temperature * oldHeatCapacity * heatScale))
            {
                burnedFuel = mixture.GetMoles(Gas.Oxygen) / Atmospherics.HydrogenBurnOxyFactor;
                if (burnedFuel > initialHydrogen)
                    burnedFuel = initialHydrogen;

                mixture.AdjustMoles(Gas.Hydrogen, -burnedFuel);
            }
            else
            {
                burnedFuel = initialHydrogen;
                mixture.AdjustMoles(Gas.Hydrogen, -burnedFuel);
                mixture.AdjustMoles(Gas.Oxygen, -burnedFuel);
                energyReleased += (Atmospherics.FireHydrogenEnergyReleased * burnedFuel * (Atmospherics.HydrogenBurnHydrogenFactor - 1));
            }

            if (burnedFuel > 0)
            {
                energyReleased += (Atmospherics.FireHydrogenEnergyReleased * burnedFuel);
                mixture.AdjustMoles(Gas.WaterVapor, burnedFuel);

                mixture.ReactionResults[GasReaction.Fire] += burnedFuel;
            }

            energyReleased /= heatScale;
            if (energyReleased > 0)
            {
                var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
                if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
                    mixture.Temperature = Math.Max(0, (temperature * oldHeatCapacity + energyReleased) / newHeatCapacity);
            }

            if (location != null)
            {
                temperature = mixture.Temperature;
                if (temperature > Atmospherics.FireMinimumTemperatureToExist)
                {
                    atmosphereSystem.HotspotExpose(location, temperature, mixture.Volume);
                }
            }

            return mixture.ReactionResults[GasReaction.Fire] != 0 ? ReactionResult.Reacting : ReactionResult.NoReaction;
        }
    }
}
