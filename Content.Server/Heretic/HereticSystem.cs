using System;
using Content.Server.Construction.Completions;
using Content.Server.GameTicking;
using Content.Shared.Eye;
using Content.Shared.Heretic.Systems;
using Content.Shared.Heretic.Components;
using Content.Shared.Stunnable;
using Content.Shared.Actions;
using Content.Shared.Toggleable;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;

namespace Content.Server.Heretic
{
    public sealed class HereticSystem : SharedHereticSystem
    {
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<HereticComponent, ToggleActionEvent>(OnToggleAction);
            SubscribeLocalEvent<HereticComponent, ComponentInit>(OnComponentInit);
        }
        private void OnToggleAction(EntityUid uid, HereticComponent component, ToggleActionEvent args)
        {
            var user = component.Owner;
            _inventorySystem.SpawnItemOnEntity(user, component.MansusHandProtoId);
        }
        private void OnComponentInit(EntityUid uid, HereticComponent component, ComponentInit args)
        {
            var user = component.Owner;
            _actionsSystem.AddAction(component.MansusHandToggleActionEntity, component.MansusHandToggleAction);
        }
    }
}