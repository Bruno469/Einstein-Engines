using Content.Server.Bible.Components;
using Content.Server.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Sword;
using Content.Shared.Damage;
using Content.Shared.Coordinates;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Timing;
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

namespace Content.Server.Sword
{
    public sealed class BlueRoseSwordSystem : SharedBlueRoseSwordComponent
    {
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
                    // AttemptSummon((uid, component), args.User, userXform);
                },
                Text = Loc.GetString("sword-reinforced-verb"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void OnReinforce(Entity<BlueRoseSwordComponent> ent, ref ActionReinforceArmamentEvent args)
        {
            // AttemptSummon(ent, args.Performer, Transform(args.Performer));
            Generate(Transform(args.Performer).Coordinates, ent.Comp.Radius, ent.Comp.Points, ent.Comp);
        }


        private void Generate(EntityCoordinates center, int radius, int points, BlueRoseSwordComponent component)
        {
            var angleStep = 360.0f / points;
            var angle = 0.0f;

            for (int i = 0; i < points; i++)
            {
                var offsetX = radius * Math.Cos(angle * Math.PI / 180.0f);
                var offsetY = radius * Math.Sin(angle * Math.PI / 180.0f);
                var offset = new Vector2((float)offsetX, (float)offsetY);
                var newCoords = center.Offset(offset);

                if (!IsTileBlockedBySomething(newCoords))
                {
                    Spawn(component.Prototype, newCoords);
                }

                angle += angleStep;
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
    }
}
