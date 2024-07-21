using Content.Server.GameTicking;
using Content.Shared.Eye;
using Content.Server.Bible.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Components;
using Content.Shared.Curse;

namespace Content.Server.Curse
{
    public sealed class CursesSystem : SharedCurseSystem
    {
        [Dependency] private readonly SharedEyeSystem _eye = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;

        private EntityQuery<CurseComponent> _curseQuery;
        private EntityQuery<PhysicsComponent> _physicsQuery;

        public override void Initialize()
        {
            base.Initialize();

            _curseQuery = GetEntityQuery<CurseComponent>();
            _physicsQuery = GetEntityQuery<PhysicsComponent>();

            SubscribeLocalEvent<CurseComponent, ComponentStartup>(OnCurseStartup);
            SubscribeLocalEvent<BibleUserComponent, ComponentStartup>(OnBibleUserStartup);
        }

        private void OnCurseStartup(EntityUid uid, CurseComponent component, ComponentStartup args)
        {
            // Allow this entity to be seen by other.
            var visibility = EnsureComp<VisibilityComponent>(uid);

            if (_ticker.RunLevel != GameRunLevel.PostRound)
            {
                _visibilitySystem.AddLayer((uid, visibility), (int) VisibilityFlags.Curse, false);
                _visibilitySystem.RemoveLayer((uid, visibility), (int) VisibilityFlags.Normal, false);
                _visibilitySystem.RefreshVisibility(uid, visibilityComponent: visibility);
            }

            SetCanSeeCurse(uid, true);
        }

        private void OnBibleUserStartup(EntityUid uid, BibleUserComponent component, ComponentStartup args)
        {
            SetCanSeeCurse(uid, true);
        }

        private void SetCanSeeCurse(EntityUid uid, bool canSee, EyeComponent? eyeComponent = null)
        {
            if (!Resolve(uid, ref eyeComponent, false))
                return;

            if (canSee)
                _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask | (int) VisibilityFlags.Curse, eyeComponent);
            else
                _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask & ~(int) VisibilityFlags.Curse, eyeComponent);
        }
    }
}
