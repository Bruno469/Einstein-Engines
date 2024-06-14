using Content.Server.Body.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Temperature;
using Content.Server.Clothing.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;

namespace Content.Server.Clothing.Systems;

public sealed class PlasmaManSuitSystem : EntitySystem
{
    [Dependency] private readonly TemperatureSystem _tempSys = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSys = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlasmaManSuitComponent, IsHotEvent>(TryExtinguirFire);
    }

    private void TryExtinguirFire(EntityUid uid, PlasmaManSuitComponent comp, IsHotEvent args)
    {
        // Verifica se tem cargas suficientes para ativar a superssão de fogo
        if (!EntityManager.TryGetComponent(uid, out TemperatureComponent? temperatureComponent) || comp.loads > 0)
            return;

        // pega a temperatura e ve se ta alta (teoricamente não precisa já que esse event só é ativado quando ta quente)
        if (temperatureComponent.CurrentTemperature > 373)
            _flammable.AdjustFireStacks(uid, -4);
            comp.loads -= 1;
    }
}
