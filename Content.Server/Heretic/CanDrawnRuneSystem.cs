using System.Collections.Generic;
using System.Numerics;
using Content.Shared.Tag;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.DoAfter;
using Content.Server.Atmos.Components;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Heretic.Systems;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Heretic.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Content.Shared.Physics;

namespace Content.Server.Heretic
{
    public sealed class CanDrawnRuneSystem : SharedCanDrawnRuneSystem
    {
        [Dependency] private readonly TagSystem _tag = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CanDrawnRuneComponent, InteractUsingEvent>(OnUseItem);
            SubscribeLocalEvent<CanDrawnRuneComponent, StartDrawnDoAfterEvent>(OnDoAfter);
        }

        private void OnUseItem(EntityUid uid, CanDrawnRuneComponent component, InteractUsingEvent args)
        {
            if (!_tag.HasTag(args.Used, "Write"))
            {
                return;
            }

            var location = args.ClickLocation;
            var gridUid = location.GetGridUid(EntityManager);

            if (gridUid == null)
            {
                return;
            }

            if (ValidLocation(gridUid.Value, location))
            {
                TryStartDrawn(uid, component, location, args);
            }
            else
            {
                Console.WriteLine("Localização inválida!");
            }
        }
        private bool TryStartDrawn(EntityUid uid, CanDrawnRuneComponent component, EntityCoordinates location, InteractUsingEvent args)
        {
            component.LocationToDrawn = location;

            var effect = Spawn(component.EffectProtoId, component.LocationToDrawn);
            component.ActiveEffect = effect;

            return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, component.DoAfterDuration, new StartDrawnDoAfterEvent(), uid, args.Used, uid)
            {
                BlockDuplicate = true,
                BreakOnUserMove = true,
                BreakOnTargetMove = true,
                BreakOnHandChange = true,
                NeedHand = true
            });
        }

        private void OnDoAfter(EntityUid uid, CanDrawnRuneComponent component, StartDrawnDoAfterEvent args)
        {
            if (component.ActiveEffect == null)
                return;

            if (args.Cancelled)
            {
                if (component.ActiveEffect != null && !Deleted(component.ActiveEffect.Value))
                {
                    QueueDel(component.ActiveEffect.Value);
                }
                return;
            }

            QueueDel(component.ActiveEffect.Value);
        }

        private bool ValidLocation(EntityUid gridUid, EntityCoordinates location)
        {

            if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
            {
                return false;
            }

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
                if (_atmosphereSystem.IsTileSpace(gridUid, null, tilePosition))
                {
                    return false;
                }

                foreach (var ent in gridComp.GetAnchoredEntities(tilePosition))
                {
                    if (!physQuery.TryGetComponent(ent, out var body))
                        continue;
                    if (body.BodyType != BodyType.Static ||
                        !body.Hard ||
                        (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                        continue;

                    return false;
                }
            }
            return true;
        }
    }
}
