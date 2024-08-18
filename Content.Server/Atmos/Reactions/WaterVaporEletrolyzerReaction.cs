using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class WaterVaporElectrolysisReaction : IGasReactionEffect
    {
        public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
        {
            var initialVapor = mixture.GetMoles(Gas.WaterVapor);
            if (initialVapor <= 0)
            {
                return ReactionResult.NoReaction;
            }

            var convertedVapor = initialVapor / 2f;
            mixture.AdjustMoles(Gas.WaterVapor, -convertedVapor);

            mixture.AdjustMoles(Gas.Hydrogen, convertedVapor * 2);
            mixture.AdjustMoles(Gas.Oxygen, convertedVapor);

            return ReactionResult.Reacting;
        }
    }
}
