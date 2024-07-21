using Content.Server.PsycUpdate.Systems;
using Content.Shared.PsycUpdate;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;

namespace Content.Server.PsycUpdate.Components
{
    /// <summary>
    /// This component stores the possible contents of the backpack,
    /// which can be selected via the interface.
    /// </summary>
    [RegisterComponent, Access(typeof(UpdateCristalSystem))]
    public sealed partial class UpdateCristalComponent : Component
    {
        /// <summary>
        /// List of sets available for selection
        /// </summary>
        [DataField]
        public List<ProtoId<UpdateCristalSetPrototype>> PossibleSets = new();

        [DataField]
        public List<int> SelectedSets = new();

        [DataField]
        public SoundSpecifier ApproveSound = new SoundPathSpecifier("/Audio/Effects/rustle1.ogg");
    }
}
