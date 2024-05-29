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
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Server.Atmos;
using System.Runtime.Intrinsics;
using Content.Server.Anomaly.Components;
using Content.Shared.Anomaly.Components;
using Robust.Server.GameObjects;

namespace Content.Server.Movement.Systems;

public sealed class MothFlySystem : SharedMothFlySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    protected override bool CanEnable(EntityUid uid, MothFlyComponent component)
    {
        var uidXform = Transform(uid);
        var coordinates = uidXform.Coordinates;
        var gridUid = coordinates.GetGridUid(EntityManager);
        var pos = coordinates.ToVector2i(EntityManager, _mapManager, _transform);
        var tile = _atmosphere.GetTileMixture(gridUid, null, pos, true);
        if (tile != null)
            return true;
        return base.CanEnable(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var toDisable = new ValueList<(EntityUid Uid, MothFlyComponent Component)>();
        var query = EntityQueryEnumerator<ActiveJetpackComponent, MothFlyComponent>();

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
