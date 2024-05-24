using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Collections;
using Robust.Shared.Timing;
using Content.Shared.Species;
using Robust.Shared.Physics.Components;
using Content.Shared.Species.Components;
using Content.Shared.Damage.Systems;
using Content.Client.Atmos.EntitySystems;

namespace Content.Server.Movement.Systems;

public sealed class MothFlySystem : SharedMothFlySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;

    protected override bool CanEnable(EntityUid uid, MothFlyComponent component)
    {
        return base.CanEnable(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var toDisable = new ValueList<(EntityUid Uid, JetpackComponent Component)>();
        var query = EntityQueryEnumerator<ActiveJetpackComponent, JetpackComponent, GasTankComponent>();

        while (query.MoveNext(out var uid, out var active, out var comp))
        {
            if (_timing.CurTime < active.TargetTime)
                continue;

            active.TargetTime = _timing.CurTime + TimeSpan.FromSeconds(active.EffectCooldown);

            if (TryComp<PhysicsComponent>(uid, out var physics))
                _stamina.TakeStaminaDamage(uid, 15);
        }

        foreach (var (uid, comp) in toDisable)
        {
            SetEnabled(uid, comp, false);
        }
    }
}
