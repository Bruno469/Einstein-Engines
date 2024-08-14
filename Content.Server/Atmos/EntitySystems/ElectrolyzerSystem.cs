using Content.Server.Atmos.Components;
using Content.Server.Atmos.Reactions;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.Popups;
using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos.Visuals;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.Atmos.EntitySystems
{
    public sealed class ElectrolyzerSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly PowerReceiverSystem _power = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ElectrolyzerComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);
        }

        private void OnDeviceUpdated(EntityUid uid, ElectrolyzerComponent electrolyzer, ref AtmosDeviceUpdateEvent args)
        {
            if (!TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiver))
            {
                Logger.WarningS("electrolyzer", $"Missing ApcPowerReceiverComponent for {uid}");
                return;
            }
            if (!_power.IsPowered(uid))
            {
                Logger.InfoS("electrolyzer", $"Electrolyzer at {uid} is not powered.");
                return;
            }

            var requiredPower = electrolyzer.MinimumEnergyRequirement;
            if (powerReceiver.PowerReceived < requiredPower)
            {
                Logger.InfoS("electrolyzer", $"Not enough power for electrolyzer {uid}. Required: {requiredPower}, Available: {powerReceiver.PowerReceived}");
                return;
            }

            var environment = _atmosphereSystem.GetContainingMixture(uid, args.Grid, args.Map);
            if (environment == null)
            {
                Logger.WarningS("electrolyzer", $"No environment found for electrolyzer {uid}");
                return;
            }


            ProcessElectrolyzer(uid, environment, electrolyzer, powerReceiver);
        }

        private void ProcessElectrolyzer(EntityUid uid, GasMixture environment, ElectrolyzerComponent electrolyzer, ApcPowerReceiverComponent powerReceiver)
        {
            powerReceiver.NetworkLoad.ReceivingPower -= electrolyzer.MinimumEnergyRequirement;

            foreach (var effect in electrolyzer.Effects)
            {
                effect.React(environment, null, _atmosphereSystem, 1.0f);
            }
            Logger.InfoS("electrolyzer", $"Electrolyzer {uid} processed successfully.");
        }
    }
}
