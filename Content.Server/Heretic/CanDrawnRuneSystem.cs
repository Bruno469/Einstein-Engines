using System.Collections.Generic;
using System.Numerics;
using Content.Shared.Tag;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.DoAfter;
using Content.Server.Atmos.Components;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Heretic.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Content.Shared.Physics;

namespace Content.Server.Heretic
{
    public sealed class CanDrawnRuneSystem : EntitySystem
    {
        [Dependency] private readonly TagSystem _tag = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CanDrawnRuneComponent, AfterInteractEvent>(OnUseItem);
            SubscribeLocalEvent<CanDrawnRuneComponent, StartDrawnDoAfterEvent>(OnDoAfter);
        }

        private void OnUseItem(EntityUid uid, CanDrawnRuneComponent component, AfterInteractEvent args)
        {
            if (!_tag.HasTag(args.Used, "Write"))
                return;

            var location = args.ClickLocation.Offset(new Vector2(-0.5f, -0.5f));
            var gridUid = location.GetGridUid(EntityManager);

            if (gridUid == null)
                return;

            if (ValidLocation(gridUid.Value, location))
            {
                TryStartDrawn(uid, component, location, args);
            }
        }

        private bool TryStartDrawn(EntityUid uid, CanDrawnRuneComponent component, EntityCoordinates location, AfterInteractEvent args)
        {
            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.DoAfterDuration, new StartDrawnDoAfterEvent(), uid, args.Target, uid)
            {
                BlockDuplicate = true,
                BreakOnUserMove = true,
                BreakOnTargetMove = true,
                BreakOnHandChange = true,
                NeedHand = true
            };

            component.LocationToDrawn = location;
            return _doAfter.TryStartDoAfter(doAfterArgs);
        }

        private void OnDoAfter(EntityUid uid, CanDrawnRuneComponent component, StartDrawnDoAfterEvent args)
        {
            var effect = Spawn(component.EffectProtoId, component.LocationToDrawn);

            if (args.Cancelled)
            {
                if (Deleted(effect) || Terminating(effect))
                    return;

                QueueDel(effect);
                return;
            }

            if (args.Target is not { } target)
                return;
        }

        private bool ValidLocation(EntityUid gridUid, EntityCoordinates location)
        {
            if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
                return false;

            if (!TryComp<MapAtmosphereComponent>(gridUid, out var gridAtmoComp))
                return false;

            var grid = _mapManager.GetGrid(gridUid);
            var gridBounds = gridComp.LocalAABB;

            var checkArea = new List<EntityCoordinates>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var offset = new Vector2(x, y);
                    var tile = location.Offset(offset);
                    checkArea.Add(tile);
                }
            }

            var physQuery = GetEntityQuery<PhysicsComponent>();

            foreach (var tile in checkArea)
            {
                var tilePosition = new Vector2i((int)tile.X, (int)tile.Y);
                if (!_atmosphereSystem.IsTileSpace(gridUid, null, tilePosition))
                    return false;

                foreach (var ent in gridComp.GetAnchoredEntities(tilePosition))
                {
                    if (physQuery.TryGetComponent(ent, out var body))
                    {
                        if (body.BodyType != BodyType.Static ||
                            !body.Hard ||
                            (body.CollisionLayer & (int)CollisionGroup.Impassable) != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
