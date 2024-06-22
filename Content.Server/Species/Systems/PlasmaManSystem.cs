using Content.Server.Body.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Temperature;
using Content.Server.Species.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;

namespace Content.Server.Species.Systems;

public sealed class PlasmaManSystem : EntitySystem
{
    [Dependency] private readonly TemperatureSystem _tempSys = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSys = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlasmaManComponent, IsHotEvent>(TryExtinguirFire);
    }

    private void TryExtinguirFire(EntityUid uid, PlasmaManComponent comp, IsHotEvent args)
    {
        int loads = 3;
        // Verifica se tem cargas suficientes para ativar a supressão de fogo
        if (!EntityManager.TryGetComponent(uid, out TemperatureComponent? temperatureComponent) || loads > 0)
            return;

        // pega a temperatura e ve se ta alta (teoricamente não precisa já que esse event só é ativado quando ta quente) [Só é ativado quando o ITEM ta quente MULA]
        // pega o componente flammable pra apagar o fogo (teoricamente posso usar pra verificar a quantidade de fire stack ja que ele fica defifinido dessa parte do codigo)
        if (!EntityManager.TryGetComponent(uid, out FlammableComponent? flammable))
            return;

        if (flammable.FireStacks >= 1)
            _flammable.Extinguish(uid, flammable);
            loads -= 1;
    }
}
