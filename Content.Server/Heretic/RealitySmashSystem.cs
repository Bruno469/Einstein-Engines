using Content.Server.GameTicking;
using Content.Server.DoAfter;
using Content.Shared.Eye;
using Content.Shared.Heretic.Systems;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Shared.Heretic.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Content.Shared.Coordinates;

namespace Content.Server.Heretic
{
    public sealed class RealitySmashSystem : SharedRealitySmashSystem
    {
        [Dependency] private readonly SharedEyeSystem _eye = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
        [Dependency] private readonly MetaDataSystem _metaData = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;

        private EntityQuery<HereticComponent> _HereticQuery;

        public override void Initialize()
        {
            base.Initialize();

            _HereticQuery = GetEntityQuery<HereticComponent>();

            SubscribeLocalEvent<RealitySmashComponent, InteractHandEvent>(OnInteract);
            SubscribeLocalEvent<RealitySmashComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<RealitySmashComponent, ColletedDoAfterEvent>(OnDoAfter);
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
        }

        private void OnUserStartup(EntityUid uid, HereticComponent component, ComponentStartup args)
        {
            // Torna as RealitySmash visiveis para os heretic
            SetCanSee(uid, true);
        }

        public void SetCanSee(EntityUid uid, bool canSee, EyeComponent? eyeComponent = null)
        {
            if (!Resolve(uid, ref eyeComponent, false))
                return;

            // A bool canSee basicamente defini se é visivel da pra usar pra controlar a visibilidade das RealitySmash tanto pra heretic quanto pra tripulação (vamos precisar de um AfterUser para controlar depois de ser usada )
            if (canSee)
                _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask | (int) VisibilityFlags.RealitySmash, eyeComponent);
            else
                _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask & ~(int) VisibilityFlags.RealitySmash, eyeComponent);
        }

        private void OnInteract(EntityUid uid, RealitySmashComponent component, InteractHandEvent args)
        {
            if (component.IsUsed) return;
            if (args.Handled) return;
            args.Handled = TryStartCollet(uid, component, args);
        }
        private bool TryStartCollet(EntityUid uid, RealitySmashComponent component, InteractHandEvent args)
        {
            return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, component.DoAfterDuration, new ColletedDoAfterEvent(),
                uid, args.Target, uid)
                {
                    BlockDuplicate = true,
                    BreakOnUserMove = true,
                    BreakOnTargetMove = true,
                    BreakOnHandChange = true,
                    NeedHand = true
                });
        }
        private void OnDoAfter(EntityUid uid, RealitySmashComponent component, ColletedDoAfterEvent args)
        {
            if (args.Cancelled)
                return;

            if (args.Target is not { } target)
                return;

            if (Deleted(uid) || Terminating(uid))
                return;


            var coords = _transformSystem.GetGridOrMapTilePosition(uid);
            Spawn("ProfundRealitySmash", new EntityCoordinates(uid, coords));
            QueueDel(uid);
        }
    }
}
