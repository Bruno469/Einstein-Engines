using Content.Server.Administration.Logs;
using Content.Server.Effects;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Movement.Components;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Content.Shared.Flamethrower;

namespace Content.Server.Projectiles;

public sealed class FireMakeSystem : SharedFireMakeSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ColorFlashEffectSystem _color = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly GunSystem _guns = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly GasTankSystem _gasTank = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FireMakerComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<FireMakerComponent, PrototypesReloadedEventArgs>(OnPrototypeLoad);
    }
    // AO SPAWNAR PROCURA A ARMA QUE ATIROU E DEPOIS MUDA OS PROPRIOS PARAMENTROS DE GAS (esse burro acha que vai funcionar) [mas cara eu não vejo por que não funcionaria] {com certeza tem forma de fazer isso melhor mas vou ver ser funciona assim, resumindo it's just a test}
    private void OnLoad(EntityUid flamethrower, EntityUid projetil)
    {
        var gas = GetGas(flamethrower);

    }

    private void OnPrototypeLoad(EntityUid uid, FireMakerComponent component, PrototypesReloadedEventArgs args)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        OnLoad(component.Weapon, uid);
    }

    private void OnStartCollide(EntityUid uid, FireMakerComponent component, ref StartCollideEvent args)
    {

    }

    private Entity<GasTankComponent>? GetGas(EntityUid uid)
    {
        if (!Container.TryGetContainer(uid, FlamethrowerComponent.TankSlotId, out var container) ||
            container is not ContainerSlot slot || slot.ContainedEntity is not {} contained)
            return null;

        return TryComp<GasTankComponent>(contained, out var gasTank) ? (contained, gasTank) : null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FireMakerComponent, JetpackComponent, GasTankComponent>();

        while (query.MoveNext(out var uid, out var active, out var comp, out var gasTankComp))
        {
            if (_timing.CurTime < active.TargetTime)
                continue;
            var gasTank = (uid, gasTankComp);
            active.TargetTime = _timing.CurTime + TimeSpan.FromSeconds(active.EffectCooldown);
            var usedAir = _gasTank.RemoveAir(gasTank, comp.MoleUsage);

            if (usedAir == null)
                continue;
        }
    }
}
