using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(HereticsRuleSystem))]
public sealed partial class HereticsRuleComponent : Component
{
    /// <summary>
    /// The amount of time between each check for command check.
    /// </summary>
    [DataField]
    public TimeSpan TimerWait = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Stores players minds
    /// </summary>
    [DataField]
    public Dictionary<string, EntityUid> Heretics = new();

    [DataField]
    public ProtoId<AntagPrototype> HereticPrototypeId = "Heretic";

    /// <summary>
    /// Min players needed for Revolutionary gamemode to start.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinPlayers = 30;

    /// <summary>
    /// Max Head Revs allowed during selection.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxHeretics = 3;

    /// <summary>
    /// The amount of Head Revs that will spawn per this amount of players.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int PlayersPerHeretic = 15;

    /// <summary>
    /// The gear head revolutionaries are given on spawn.
    /// </summary>
    [DataField]
    public List<EntProtoId> StartingGear = new()
    {
        "heretichandbook",
        "ClothingEyesGlassesSunglasses"
    };

    /// <summary>
    /// The time it takes after the last heretic is dead
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ShuttleCallTime = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Start knowledge
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int StartingKnowledge = 6;
}
