using Content.Shared.Popups;

namespace Content.Shared.Curse
{
    public abstract class SharedCurseSystem : EntitySystem
    {
        [Dependency] protected readonly SharedPopupSystem Popup = default!;

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
