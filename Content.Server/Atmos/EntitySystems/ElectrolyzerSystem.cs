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
                return;

            if (!_power.IsPowered(uid))
                return;

            var requiredPower = electrolyzer.MinimumEnergyRequirement;
            if (powerReceiver.PowerReceived < requiredPower)
                return;

            var environment = _atmosphereSystem.GetContainingMixture(uid, args.Grid, args.Map);

            if (environment == null)
                return;

            React(environment, AtmosphereSysteme);
            powerReceiver.NetworkLoad.ReceivingPower -= electrolyzer.MinimumEnergyRequirement;
            Logger.InfoS("electrolyzer", $"Electrolyzer {uid} processed successfully.");
        }
    }
}
