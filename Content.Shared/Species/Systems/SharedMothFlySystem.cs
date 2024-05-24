using Content.Shared.Species.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Events;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Species;
public abstract class SharedMothFlySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothFlyComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MothFlyComponent, ToggleMothflyEvent>(OnFlyAction);
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
        // When they use the action, gib them (It's a meme lol).
        _popupSystem.PopupClient(Loc.GetString(comp.PopupText, ("name", uid)), uid, uid);
        if (TryComp<PhysicsComponent>(uid, out var physics))
            _stamina.TakeStaminaDamage(uid, 50);
    }

    private bool IsEnabled(EntityUid uid)
    {
        return HasComp<MothFlyComponent>(uid);
    }

    protected virtual bool CanEnable(EntityUid uid, MothFlyComponent component)
    {
        return true;
    }
    public void SetEnabled(EntityUid uid, MothFlyComponent component, bool enabled)
    {
        var user = component.Owner;

        if (IsEnabled(uid) == enabled ||
            enabled && !CanEnable(uid, component))
        {
            if (TryComp<PhysicsComponent>(user, out var physics))
                _physics.SetBodyStatus(user, physics, BodyStatus.OnGround);
            return;
        }

        if (TryComp<PhysicsComponent>(user, out var newphysics))
            _physics.SetBodyStatus(user, newphysics, BodyStatus.InAir);
        _movementSpeedModifier.ChangeBaseSpeed(user, 2, 2, 1);
    }

    public bool IsUserFlying(EntityUid uid)
    {
        return HasComp<MothFlyComponent>(uid);
    }
}
