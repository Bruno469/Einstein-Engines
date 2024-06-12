using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Stunnable;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Flamethrower;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Content.Server.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.GameObjects;

namespace Content.Server.Flamethrower;

public sealed class FlamethrowerSystem : SharedFlamethrowerSystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly GasTankSystem _gasTank = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GunComponent, AmmoShotEvent>(OnAmmoShot);
        SubscribeLocalEvent<FlamethrowerComponent, GunShotEvent>(OnShoot);
        SubscribeLocalEvent<FlamethrowerComponent, ContainerIsInsertingAttemptEvent>(OnContainerInserting);
    }

    private void OnContainerInserting(EntityUid uid, FlamethrowerComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (args.Container.ID != FlamethrowerComponent.TankSlotId)
            return;

        if (!TryComp<GasTankComponent>(args.EntityUid, out var gas))
            return;

        // only accept tanks if it uses gas
        if (gas.Air.TotalMoles >= component.GasUsage && component.GasUsage > 0f)
            return;

        args.Cancel();
    }

    private void OnShoot(Entity<FlamethrowerComponent> cannon, ref GunShotEvent args)
    {
        var (uid, component) = cannon;
        // require a gas tank if it uses gas
        var gas = GetGas(cannon);
        if (gas == null && component.GasUsage > 0f)
            return;

        // ignore gas stuff if the cannon doesn't use any
        if (gas == null)
            return;

        // this should always be possible, as we'll eject the gas tank when it no longer is
        var environment = _atmos.GetContainingMixture(cannon, false, true);
        var removed = _gasTank.RemoveAir(gas.Value, component.GasUsage);
        if (environment != null && removed != null)
        {
            _atmos.Merge(environment, removed);
        }

        if (gas.Value.Comp.Air.TotalMoles >= component.GasUsage)
            return;

        // eject gas tank
        _slots.TryEject(uid, FlamethrowerComponent.TankSlotId, args.User, out _);
    }
    public void OnAmmoShot(EntityUid uid, GunComponent component, AmmoShotEvent args)
    {
        var gas = GetGas(uid);
        var environment = _atmos.GetContainingMixture(uid, false, true);
        var merger = new GasMixture(1) { Temperature = 383 };
        merger.SetMoles(gas.Value, gas.Value.Comp.Air.TotalMoles);
        foreach (var projectileUid in args.FiredProjectiles)
        {
            if (_entity.TryComp<FireMakerComponent>(projectileUid, out var flamethrowerBullet))
            {
                flamethrowerBullet.Merge(environment, merger);
            }
        }
    }
    /// <summary>
    ///     Returns whether the pneumatic cannon has enough gas to shoot an item, as well as the tank itself.
    /// </summary>
    private Entity<GasTankComponent>? GetGas(EntityUid uid)
    {
        if (!Container.TryGetContainer(uid, FlamethrowerComponent.TankSlotId, out var container) ||
            container is not ContainerSlot slot || slot.ContainedEntity is not {} contained)
            return null;

        return TryComp<GasTankComponent>(contained, out var gasTank) ? (contained, gasTank) : null;
    }
}
