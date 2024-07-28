using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
///     Added to mind entities to tag that they are a Heretic.
/// </summary>
[RegisterComponent, ExclusiveAntagonist]
public sealed partial class HereticRoleComponent : AntagonistRoleComponent
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public uint Sacrifices = 0;
}
