using Content.Server.Clothing.Systems;

namespace Content.Server.Clothing.Components;

[RegisterComponent]
[Access(typeof(PlasmaManSuitSystem))]
public sealed partial class PlasmaManSuitComponent : Component
{
    [DataField("Loads")]
    public int loads = 3;
}
