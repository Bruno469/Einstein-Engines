using Content.Shared.Movement.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;
using Content.Shared.Species;
using Content.Shared.Species.Components;
using Content.Shared.Damage.Systems;
using Content.Client.Atmos.EntitySystems;
using Robust.Shared.Containers;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Movement.Systems;
using Robust.Client.GameObjects;

namespace Content.Client.Movement.Systems;

public sealed class MothFlySystems : SharedMothFlySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override bool CanEnable(EntityUid uid, MothFlyComponent component)
    {
        // No predicted atmos so you'd have to do a lot of funny to get this working.
        return false;
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        // TODO: Please don't copy-paste this I beg
        // make a generic particle emitter system / actual particles instead.
        var query = EntityQueryEnumerator<ActiveJetpackComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.TargetTime)
                continue;

            comp.TargetTime = _timing.CurTime + TimeSpan.FromSeconds(comp.EffectCooldown);

            CreateParticles(uid);
        }
    }

    private void CreateParticles(EntityUid uid)
    {
        // Don't show particles unless the user is moving.
        if (TryComp<PhysicsComponent>(uid, out var body) &&
            body.LinearVelocity.LengthSquared() < 1f)
        {
            return;
        }

        var uidXform = Transform(uid);
        var coordinates = uidXform.Coordinates;
        var gridUid = coordinates.GetGridUid(EntityManager);

        if (TryComp<MapGridComponent>(gridUid, out var grid))
        {
            coordinates = new EntityCoordinates(gridUid.Value, grid.WorldToLocal(coordinates.ToMapPos(EntityManager, _transform)));
        }
        else if (uidXform.MapUid != null)
        {
            coordinates = new EntityCoordinates(uidXform.MapUid.Value, _transform.GetWorldPosition(uidXform));
        }
        else
        {
            return;
        }

        Spawn("JetpackEffect", coordinates);
    }
}
