using Content.Server.GameTicking;
using Content.Shared.Eye;
using Content.Shared.Heretic.Systems;
using Content.Shared.Interaction;
using Content.Shared.Heretic.Components;
using Robust.Server.GameObjects;

namespace Content.Server.Heretic
{
    public sealed class RealitySmashSystem : SharedRealitySmashSystem
    {
        [Dependency] private readonly SharedEyeSystem _eye = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;

        private EntityQuery<HereticComponent> _HereticQuery;

        public override void Initialize()
        {
            base.Initialize();

            _HereticQuery = GetEntityQuery<HereticComponent>();

            SubscribeLocalEvent<RealitySmashComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<RealitySmashComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<HereticComponent, ComponentStartup>(OnUserStartup);
        }

        private void OnStartup(EntityUid uid, RealitySmashComponent component, ComponentStartup args)
        {
            // Quando spawnado deixa invisivel pra outras entitidades (talvez, apenas talvez, a luz da entidade seja possivel ver mas ae é mais facil remover a luz hihi)
            var visibility = EnsureComp<VisibilityComponent>(uid);

            if (_ticker.RunLevel != GameRunLevel.PostRound)
            {
                _visibilitySystem.AddLayer((uid, visibility), (int) VisibilityFlags.RealitySmash, false);
                _visibilitySystem.RemoveLayer((uid, visibility), (int) VisibilityFlags.Normal, false);
                _visibilitySystem.RefreshVisibility(uid, visibilityComponent: visibility);
            }

            SetCanSee(uid, true);
        }

        private void OnUserStartup(EntityUid uid, HereticComponent component, ComponentStartup args)
        {
            // Torna as RealitySmash para os heretic
            SetCanSee(uid, true);
        }

        private void SetCanSee(EntityUid uid, bool canSee, EyeComponent? eyeComponent = null)
        {
            if (!Resolve(uid, ref eyeComponent, false))
                return;

            // A bool canSee basicamente defini se é visivel da pra usar pra controlar a visibilidade das RealitySmash tanto pra heretic quanto pra tripulação (vamos precisar de um AfterUser para controlar depois de ser usada )
            if (canSee)
                _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask | (int) VisibilityFlags.RealitySmash, eyeComponent);
            else
                _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask & ~(int) VisibilityFlags.RealitySmash, eyeComponent);
        }

        private void OnInteractUsing(EntityUid uid, RealitySmashComponent component, InteractUsingEvent args)
        {
            if (component.IsUsed) return;
            if (args.Handled) return;
            //if (component.AllowedItems == null) return;
            //if (!component.AllowedItems.IsValid(args.Used, EntityManager)) return;

        }
    }
}
