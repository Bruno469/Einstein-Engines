using Content.Shared.Species.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;


namespace Content.Shared.Species;

public sealed partial class MothFlySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothFlyComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MothFlyComponent, BaseActionEvent>(OnFlyAction);
    }

    private void OnMobStateChanged(EntityUid uid, MothFlyComponent comp, MobStateChangedEvent args)
    {
        // When the mob changes state, check if they're dead and give them the action if so. 
        if (!TryComp<MobStateComponent>(uid, out var mobState))
            return;

        if (!_protoManager.TryIndex<EntityPrototype>(comp.ActionPrototype, out var actionProto))
            return;


        foreach (var allowedState in comp.AllowedStates)
        {
            if (allowedState == mobState.CurrentState)
            {
                // The mob should never have more than 1 state so I don't see this being an issue
                _actionsSystem.AddAction(uid, ref comp.ActionEntity, comp.ActionPrototype);
                return;
            }
        }
    }

    private void OnFlyAction(EntityUid uid, MothFlyComponent comp, BaseActionEvent args)
    {
        // When they use the action, gib them.
        _popupSystem.PopupClient(Loc.GetString(comp.PopupText, ("name", uid)), uid, uid);
        _bodySystem.GibBody(uid, true);
    }
}
