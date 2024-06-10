using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared.Flamethrower;

public abstract class SharedFlamethrowerSystem : EntitySystem
{
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlamethrowerComponent, AttemptShootEvent>(OnAttemptShoot);
    }

    private void OnAttemptShoot(EntityUid uid, FlamethrowerComponent component, ref AttemptShootEvent args)
    {
        // if the cannon doesn't need gas then it will always predict firing
        if (component.GasUsage == 0f)
            return;

        // pneumatic cannon usually doesn't shoot bullets
        args.ThrowItems = true;

        // we don't have atmos on shared, so just predict by the existence of a slot item
        // server will handle auto ejecting/not adding the slot item if it doesnt have enough gas,
        // so this won't mispredict
        if (!Container.TryGetContainer(uid, FlamethrowerComponent.TankSlotId, out var container) ||
            container is not ContainerSlot slot || slot.ContainedEntity is null)
        {
            args.Cancelled = true;
        }
    }
}
