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
using Content.Shared.Movement.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;
using Content.Shared.Stunnable;

namespace Content.Shared.Species;
public abstract partial class SharedMothFlySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothFlyComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MothFlyComponent, FlyActionEvent>(OnFlyAction);
        SubscribeLocalEvent<MothFlyComponent, ComponentInit>(OnRoundStart);
        SubscribeLocalEvent<MothFlyComponent, StunnedEvent>(HardStopFly);
    }

    public void OnRoundStart(EntityUid uid, MothFlyComponent comp, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, ref comp.ActionEntity, comp.ActionPrototype);
        return;
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

    private void OnFlyAction(EntityUid uid, MothFlyComponent comp, FlyActionEvent args)
    {
        // When they use the action, gib them (It's a meme lol).
        bool canFly = CanEnable(uid, comp);
        SetEnabled(uid, comp, canFly);
    }

    private bool IsEnabled(EntityUid uid)
    {
        return HasComp<ActiveJetpackComponent>(uid);
    }

    protected virtual bool CanEnable(EntityUid uid, MothFlyComponent component)
    {
        if (IsEnabled(uid))
            return false;
        return false;
    }

    public void SetEnabled(EntityUid uid, MothFlyComponent component, bool enabled)
    {
        var user = component.Owner;

        if (IsEnabled(uid) == true ||
            enabled && !CanEnable(uid, component))
        {
            if (TryComp<PhysicsComponent>(user, out var physics))
                _physics.SetBodyStatus(user, physics, BodyStatus.OnGround);
                RemComp<ActiveJetpackComponent>(uid);
                RemComp<CanMoveInAirComponent>(uid);
                _movementSpeedModifier.ChangeBaseSpeed(user, MovementSpeedModifierComponent.DefaultBaseWalkSpeed, MovementSpeedModifierComponent.DefaultBaseSprintSpeed, MovementSpeedModifierComponent.DefaultAcceleration);
                _movementSpeedModifier.ChangeFriction(user, MovementSpeedModifierComponent.DefaultFriction, MovementSpeedModifierComponent.DefaultFrictionNoInput, MovementSpeedModifierComponent.DefaultAcceleration);
                _popupSystem.PopupClient(Loc.GetString(component.StopPopupText, ("name", uid)), uid, uid);
            return;
        }

        if (TryComp<PhysicsComponent>(user, out var newphysics))
            _physics.SetBodyStatus(user, newphysics, BodyStatus.InAir);
            AddComp<ActiveJetpackComponent>(uid);
            AddComp<CanMoveInAirComponent>(uid);
            _movementSpeedModifier.ChangeBaseSpeed(user, 5.0f, 8.0f, 30f);
            _movementSpeedModifier.ChangeFriction(user, 10.0f, 5.0f, 30f);
            _popupSystem.PopupClient(Loc.GetString(component.PopupText, ("name", uid)), uid, uid);
    }

    private HardStopFly(EntityUid uid, StunnedEvent args)
    {
        if (!HasComp<CanMoveInAirComponent>(uid))
            return;

        //continue codgio
    }

    public bool IsUserFlying(EntityUid uid)
    {
        return HasComp<ActiveJetpackComponent>(uid);
    }

    public sealed partial class FlyActionEvent : InstantActionEvent { }
}
