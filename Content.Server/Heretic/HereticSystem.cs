using System;
using Content.Server.Construction.Completions;
using Content.Server.GameTicking;
using Content.Shared.Eye;
using Content.Shared.Heretic.Systems;
using Content.Shared.Heretic.Components;
using Content.Shared.Actions;
using Content.Shared.Hands.Components;
using Content.Shared.Item;
using Content.Shared.Toggleable;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Player;

namespace Content.Server.Heretic
{
    public sealed class HereticSystem : SharedHereticSystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<HereticComponent, ToggleActionEvent>(OnToggleAction);
            SubscribeLocalEvent<HereticComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<HereticComponent, HereticResearchMenuActionEvent>(OnOpenResearch);
        }

        private void OnToggleAction(EntityUid uid, HereticComponent component, ToggleActionEvent args)
        {
            var user = component.Owner;
            PickupOrDelete(user, component, false);
        }

        private void OnComponentInit(EntityUid uid, HereticComponent component, ComponentInit args)
        {
            var user = component.Owner;
            _actionsSystem.AddAction(user, ref component.MansusHandActionEntity, component.MansusHandToggleAction);
            _actionsSystem.TryGetActionData( component.MansusHandActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actionsSystem.StartUseDelay(component.MansusHandActionEntity);
        }

        private void OnOpenResearch(EntityUid uid, HereticComponent component, HereticResearchMenuActionEvent args)
        {
            if (!TryComp<ActorComponent>(args.Performer, out var actor) ||
                !_uiSystem.TryGetUi(uid, SharedCrayonComponent.CrayonUiKey.Key, out var ui))
            {
                return;
            }

            _uiSystem.ToggleUi(ui, actor.PlayerSession);
            if (ui.SubscribedSessions.Contains(actor.PlayerSession))
            {
                _uiSystem.SetUiState(ui, new CrayonBoundUserInterfaceState(component.SelectedState, component.SelectableColor, component.Color));
            }
        }

        private void PickupOrDelete(EntityUid? uid, HereticComponent component, bool checkActionBlocker = true, bool animateUser = false, bool animate = false, HandsComponent? handsComp = null, ItemComponent? item = null)
        {
            var user = component.Owner;

            if (!HasComp<TransformComponent>(user))
                return;

            var xform = Transform(user);
            var mapCoords = _transform.GetMapCoordinates(xform);
            var itemToSpawn = Spawn(component.MansusHandProtoId, mapCoords);

            if (uid == null
                || !Resolve(uid.Value, ref handsComp, false)
                || !_handsSystem.TryGetEmptyHand(uid.Value, out var hand, handsComp)
                || !_handsSystem.TryPickup(uid.Value, itemToSpawn, hand, checkActionBlocker, animate, handsComp, item))
            {
                QueueDel(itemToSpawn);
            } else {
                _actionsSystem.TryGetActionData( component.MansusHandActionEntity, out var actionData );
                if (actionData is { UseDelay: not null })
                    _actionsSystem.StartUseDelay(component.MansusHandActionEntity);
            }
        }
    }
}
