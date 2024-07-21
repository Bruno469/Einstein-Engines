using Content.Client.Movement.Systems;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.Curse;
using Robust.Client.Console;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Curse
{
    public sealed class CursesSystem : SharedCurseSystem
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        private bool _curseVisibility = true;

        private bool CurseVisibility
        {
            get => _curseVisibility;
            set
            {
                if (_curseVisibility == value)
                {
                    return;
                }

                _curseVisibility = value;

                var query = AllEntityQuery<CurseComponent, SpriteComponent>();
                while (query.MoveNext(out var uid, out _, out var sprite))
                {
                    sprite.Visible = value || uid == _playerManager.LocalEntity;
                }
            }
        }


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CurseComponent, ComponentStartup>(OnStartup);
        }

        private void OnStartup(EntityUid uid, CurseComponent component, ComponentStartup args)
        {
            if (TryComp(uid, out SpriteComponent? sprite))
                sprite.Visible = CurseVisibility || uid == _playerManager.LocalEntity;
        }
    }
}
