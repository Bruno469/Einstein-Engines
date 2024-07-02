using Content.Server.Temperature.Systems;
using Content.Shared.ActionBlocker;
using Content.Server.Species.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Actions.Events;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Containers;

namespace Content.Server.Species.Systems;

public sealed class PlasmaManSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly TemperatureSystem _tempSys = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSys = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlasmaManComponent, FireStarterActionEvent>(TryExtinguirFire);
    }

    private void TryExtinguirFire(EntityUid uid, PlasmaManComponent comp, FireStarterActionEvent args)
    {
        // Verifica se tem cargas suficientes para ativar a supressão de fogo
        if (!TryGetClothingSlotEntity(uid, comp, out var entity))
            return;
        if (entity.Value.PlasmaManSuit.loads <= 0)
            return;

        // pega a temperatura e ve se ta alta (teoricamente não precisa já que esse event só é ativado quando ta quente) [Só é ativado quando o ITEM ta quente MULA]
        // pega o componente flammable pra apagar o fogo (teoricamente posso usar pra verificar a quantidade de fire stack ja que ele fica defifinido nessa parte do codigo)
        if (!EntityManager.TryGetComponent(uid, out FlammableComponent? flammable))
            return;

        if (flammable.FireStacks >= 1)
            _flammable.Extinguish(uid, flammable);
            entity.Value.PlasmaManSuit.loads -= 1;
    }

    private bool TryGetClothingSlotEntity(EntityUid uid, PlasmaManComponent component, [NotNullWhen(true)] out EntityUid? slotEntity)
    {
        slotEntity = null;

        if (!_container.TryGetContainingContainer(uid, out var container))
            return false;
        var user = container.Owner;

        if (!_inventory.TryGetContainerSlotEnumerator(user, out var enumerator, component.TargetSlot))
            return false;

        while (enumerator.NextItem(out var item))
        {
            if (component.ProviderWhitelist == null || !component.ProviderWhitelist.IsValid(item, EntityManager))
                continue;

            slotEntity = item;
            return true;
        }

        return false;
    }
}
