using System;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Storage.Components;
using Content.Shared.Weapons.Melee.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Melee
{
    public sealed class RechargeBeltSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly INetManager _netManager = default!;
        [Dependency] private readonly SharedStorageSystem _storageSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly MetaDataSystem _metadata = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RechargeBeltComponent, MapInitEvent>(OnInit);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            // Atualize a consulta para encontrar componentes de armazenamento
            var query = EntityQueryEnumerator<RechargeBeltComponent>();

            while (query.MoveNext(out var uid, out var recharge))
            {
                // Verifique se NextCharge tem um valor
                if (recharge.NextCharge.HasValue && recharge.NextCharge.Value > _timing.CurTime)
                    continue;

                // Substitua FillStorage com o método apropriado se necessário
                // Exemplo fictício de uso de um método de armazenamento:
                // _storageSystem.Recharge(uid);

                if (recharge.NextCharge.HasValue)
                {
                    recharge.NextCharge = recharge.NextCharge.Value + TimeSpan.FromSeconds(recharge.RechargeCooldown);
                }
                else
                {
                    recharge.NextCharge = _timing.CurTime + TimeSpan.FromSeconds(recharge.RechargeCooldown);
                }

                Dirty(recharge);
            }
        }

        private void OnInit(EntityUid uid, RechargeBeltComponent component, MapInitEvent args)
        {
            component.NextCharge = _timing.CurTime;
            Dirty(component);
        }
    }
}
