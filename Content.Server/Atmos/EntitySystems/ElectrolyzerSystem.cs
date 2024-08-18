using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.Reactions;
using Content.Server.Atmos.Piping.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Atmos
{
    public sealed class ElectrolyzerSystem : EntitySystem
    {
        private const float BaseEfficiency = 1.0f;
        private const float MinTemperatureForMaxEfficiency = 300.0f;
        private const float MaxTemperatureForMaxEfficiency = 800.0f;

        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ElectrolyzerComponent, AtmosDeviceUpdateEvent>(OnAtmosDeviceUpdate);
        }

        private void OnAtmosDeviceUpdate(EntityUid uid, ElectrolyzerComponent component, AtmosDeviceUpdateEvent args)
        {
            var mixture = _atmosphereSystem.GetContainingMixture(uid);
            if (mixture == null)
                return;

            var gasReaction = GetElectrolysisReactionPrototype();
            if (gasReaction == null)
                return;

            if (mixture.Temperature < gasReaction.MinimumTemperatureRequirement ||
                mixture.Temperature > gasReaction.MaximumTemperatureRequirement)
            {
                return;
            }

            float efficiency = CalculateEfficiencyBasedOnTemperature(mixture.Temperature);

            var reactionResult = gasReaction.React(mixture, null, _atmosphereSystem, efficiency);

            if (reactionResult.HasFlag(ReactionResult.StopReactions))
            {

            }
        }

        private GasReactionPrototype? GetElectrolysisReactionPrototype()
        {
            return _prototypeManager.Index<GasReactionPrototype>("Electrolyzer");
        }

        private float CalculateEfficiencyBasedOnTemperature(float temperature)
        {
            if (temperature < MinTemperatureForMaxEfficiency)
            {
                return BaseEfficiency * (temperature / MinTemperatureForMaxEfficiency);
            }
            else if (temperature > MaxTemperatureForMaxEfficiency)
            {
                return BaseEfficiency * (MaxTemperatureForMaxEfficiency / temperature);
            }
            return BaseEfficiency;
        }
    }
}
