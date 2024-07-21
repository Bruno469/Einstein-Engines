using System.Runtime.CompilerServices;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Melee;

public sealed class RechargeBeltSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly StorageSystem _storageSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RechargeBeltComponent, MapInitEvent>(OnInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<RechargeBeltComponent, StorageComponent>();

        while (query.MoveNext(out var uid, out var recharge, out var storage))
        {

            if (recharge.NextCharge > _timing.CurTime)
                continue;

            _storageSystem.FillStorage(uid, storage);

            recharge.NextCharge = recharge.NextCharge.Value + TimeSpan.FromSeconds(recharge.RechargeCooldown);
            Dirty(recharge);
        }
    }

    private void OnInit(EntityUid uid, RechargeBeltComponent component, MapInitEvent args)
    {
        component.NextCharge = _timing.CurTime;
        Dirty(component);
    }

    public void Reset(EntityUid uid, RechargeBeltComponent? recharge = null)
    {
        if (!Resolve(uid, ref recharge, false))
            return;

        if (recharge.NextCharge == null || recharge.NextCharge < _timing.CurTime)
        {
            recharge.NextCharge = _timing.CurTime + TimeSpan.FromSeconds(recharge.RechargeCooldown);
            Dirty(recharge);
        }
    }
}
