using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(HereticsRuleSystem))]
public sealed partial class HereticsRuleComponent : Component
{
    /// <summary>
    /// Stores players minds
    /// </summary>
    [DataField]
    public Dictionary<string, EntityUid> Heretics = new();

    [DataField]
    public ProtoId<AntagPrototype> HereticPrototypeId = "Heretic";

    /// <summary>
    /// Min players needed for heretic gamemode to start.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinPlayers = 30;

    /// <summary>
    /// Max heretics allowed during selection.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxHeretics = 3;

    /// <summary>
    /// The amount of heretics that will spawn per this amount of players.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int PlayersPerHeretic = 15;

    /// <summary>
    /// The time it takes after the last heretic is dead
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ShuttleCallTime = TimeSpan.FromMinutes(3);

    [DataField("RealitySmashSpawnerPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string RealitySmashSpawnerPrototype = "RealitySmash";

    /// <summary>
    /// Start knowledge
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int StartingKnowledge = 6;
}
