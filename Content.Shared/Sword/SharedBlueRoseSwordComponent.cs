using Content.Shared.Popups;

namespace Content.Shared.Sword
{
    public abstract class SharedBlueRoseSwordComponent : EntitySystem
    {
        [Dependency] protected readonly SharedPopupSystem Popup = default!;

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
