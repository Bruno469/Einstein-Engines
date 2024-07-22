using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Bible.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.Atmos.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.Sword;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Content.Shared.Spider;
using Content.Shared.Maps;
using Content.Shared.Movement.Components;
using Robust.Shared.Map;
using Vector2 = System.Numerics.Vector2;
using Content.Shared.Coordinates;


namespace Content.Server.Sword
{
    public sealed class BlueRoseSwordSystem : SharedBlueRoseSwordComponent
    {
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ActionBlockerSystem _blocker = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly InventorySystem _invSystem = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly UseDelaySystem _delay = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BlueRoseSwordComponent, GetVerbsEvent<AlternativeVerb>>(AddReinforceVerb);
            SubscribeLocalEvent<BlueRoseSwordComponent, GetItemActionsEvent>(GetReinforceAction);
            SubscribeLocalEvent<BlueRoseSwordComponent, ActionReinforceArmamentEvent>(OnReinforce);
        }

        private void GetReinforceAction(EntityUid uid, BlueRoseSwordComponent component, GetItemActionsEvent args)
        {
            if (component.AlreadyReinforce)
                return;

            args.AddAction(ref component.ReinforceActionEntity, component.ReinforceAction);
        }

        private void AddReinforceVerb(EntityUid uid, BlueRoseSwordComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess || component.AlreadyReinforce)
                return;

            if (component.RequiresBibleUser && !HasComp<BibleUserComponent>(args.User))
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    if (!TryComp<TransformComponent>(args.User, out var userXform))
                        return;

                    Generate(args.User.ToCoordinates(), component.Radius, component.Points, component);
                },
                Text = Loc.GetString("sword-reinforced-verb"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void OnReinforce(Entity<BlueRoseSwordComponent> ent, ref ActionReinforceArmamentEvent args)
        {
            Generate(Transform(args.Performer).Coordinates, ent.Comp.Radius, ent.Comp.Points, ent.Comp);
        }

        private void Generate(EntityCoordinates center, int radius, int points, BlueRoseSwordComponent component)
        {
            var stepSize = 1;
            for (int x = -radius; x <= radius; x += stepSize)
            {
                for (int y = -radius; y <= radius; y += stepSize)
                {
                    var distance = Math.Sqrt(x * x + y * y);
                    if (distance <= radius)
                    {
                        var offset = new Vector2(x, y);
                        var newCoords = center.Offset(offset);
                        var tilePosition = new Vector2i((int)newCoords.X, (int)newCoords.Y);

                        if (!IsTileBlockedBySomething(newCoords) || !_atmos.IsTileSpace(null, null, tilePosition))
                        {
                            Spawn(component.Prototype, newCoords);
                        }
                    }
                }
            }
        }

        private bool IsTileBlockedBySomething(EntityCoordinates coords)
        {
            foreach (var entity in coords.GetEntitiesInTile())
            {
                if (HasComp<FrictionContactsComponent>(entity))
                    return true;
            }
            return false;
        }

        //private bool IsTileOccupiedByEntities(Vector2i tilePosition, EntityUid gridId)
        //{
        //    var tileRef = new TileRef(tilePosition, gridId);
        //    var entitiesInTile = _transform.GetEntitiesInTile(tileRef);
        //    return entitiesInTile.Any(entity => !HasComp<PhysicsComponent>(entity));
        //}
    }
}
