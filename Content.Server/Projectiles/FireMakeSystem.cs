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
using Robust.Shared.Utility;

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
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FireMakerComponent, StartCollideEvent>(OnStartCollide);
    }
    // AO SPAWNAR PROCURA A ARMA QUE ATIROU E DEPOIS MUDA OS PROPRIOS PARAMENTROS DE GAS (esse burro acha que vai funcionar) [mas cara eu não vejo por que não funcionaria] {com certeza tem forma de fazer isso melhor mas vou ver ser funciona assim, resumindo it's just a test} [[Tá... eu movi TUDO que eu falei aqui para a arma basicamente agora é só uma verificação e acredito que fica mais leve em questão de processamento...]]

    private void OnStartCollide(EntityUid uid, FireMakerComponent component, ref StartCollideEvent args)
    {

    }
    private Entity<GasTankComponent>? GetGas(EntityUid uid)
    {
        if (!_containerSystem.TryGetContainer(uid, FlamethrowerComponent.TankSlotId, out var container) ||
            container is not ContainerSlot slot || slot.ContainedEntity is not {} contained)
            return null;

        return TryComp<GasTankComponent>(contained, out var gasTank) ? (contained, gasTank) : null;
    }

    // Basicamente aqui vai ficar responsavel pela eliminação do gas dentro do projetil para o tile em que ele se encontra, estou usando update por enquanto pois não conheço um event que ativa de tile em tile TODO: procurar um event pra isso (provavelmente não tem)
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FireMakerComponent, GasTankComponent>();

        while (query.MoveNext(out var uid, out var comp, out var gasTankComp))
        {
            if (_timing.CurTime < comp.TargetTime)
                continue;

            var gasTank = (uid, gasTankComp);
            comp.TargetTime = _timing.CurTime + TimeSpan.FromSeconds(comp.EffectCooldown);
            _entity.TryGetComponent<FlamethrowerComponent>(comp.Shooter, out var flamethrower);

            if (flamethrower == null)
                return;

            var usedAir = _gasTank.RemoveAir(gasTank, flamethrower.GasUsage);
        }
    }
}
