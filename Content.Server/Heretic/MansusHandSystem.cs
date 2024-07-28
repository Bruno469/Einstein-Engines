using System;
using Content.Server.GameTicking;
using Content.Shared.Eye;
using Content.Shared.Heretic.Components;
using Robust.Server.GameObjects;

namespace Content.Server.Heretic
{
    public sealed class MansusHandSystem : SharedMansusHandSystem
    {

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MansusHandComponent, GotEquippedHandEvent>(OnTakeInHand);
        }

        private void OnTakeInHand(EntityUid uid, MansusHandComponent component, GotEquippedHandEvent args)
        {
            var user = args.user;
            var stun = EnsureComp<StunProviderComponent>(user);
        }
    }
}
