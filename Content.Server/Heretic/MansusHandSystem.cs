using System;
using Content.Server.Construction.Completions;
using Content.Server.GameTicking;
using Content.Shared.Eye;
using Content.Shared.Heretic.Components;
using Content.Shared.Stunnable;
using Content.Shared.Heretic.Systems;
using Content.Shared.Interaction;
using Content.Shared.Hands;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Server.GameObjects;

namespace Content.Server.Heretic
{
    public sealed class MansusHandSystem : SharedMansusHandSystem
    {
        [Dependency] private readonly SharedStunSystem _stun = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MansusHandComponent, GotEquippedHandEvent>(OnTakeInHand);
            SubscribeLocalEvent<MansusHandComponent, AfterInteractEvent>(OnAfterInteract);
        }

        private void OnTakeInHand(EntityUid uid, MansusHandComponent component, GotEquippedHandEvent args)
        {
            var user = component.Owner;
        }

        private void OnAfterInteract(EntityUid uid, MansusHandComponent component, AfterInteractEvent args)
        {
            if (args.Handled || args.Target is not { } target)
                return;
            TryStun(uid, target, args.User, component);
        }

        public void TryStun(EntityUid uid, EntityUid target, EntityUid user, MansusHandComponent component)
        {
            _audio.PlayPvs(component.Sound, uid);

            _stun.TryParalyze(target, component.StunTime, refresh: false);
            DeleteEntity(uid);
        }

        private void DeleteEntity(EntityUid uid)
        {
            if (Deleted(uid) || Terminating(uid))
                return;
            QueueDel(uid);
        }
    }
}
