using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class BZFormationReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialPlasma = mixture.GetMoles(Gas.Plasma);
        var initialN2O = mixture.GetMoles(Gas.NitrousOxide);

        if (initialPlasma < 0.02f || initialN2O < 0.01f)
        {
            return ReactionResult.NoReaction;
        }

        var plasmaConsumed = Math.Min(initialPlasma, initialN2O * 2);
        var n2oConsumed = plasmaConsumed / 2;

        mixture.AdjustMoles(Gas.Plasma, -plasmaConsumed);
        mixture.AdjustMoles(Gas.NitrousOxide, -n2oConsumed);

        var bzProduced = plasmaConsumed * 1.5f;
        mixture.AdjustMoles(Gas.BZ, bzProduced);

        var energyReleased = bzProduced * 50f;
        var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
        var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);

        if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
        {
            mixture.Temperature = (mixture.Temperature * oldHeatCapacity + energyReleased) / newHeatCapacity;
        }

        return ReactionResult.Reacting;
    }
}
