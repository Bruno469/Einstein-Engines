using Content.Shared.Lathe;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Heretic.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedHereticResearchSystem), typeof(SharedLatheSystem)), AutoGenerateComponentState]
public sealed partial class HereticResearchDatabaseComponent : Component
{
    /// <summary>
    /// A main discipline that locks out other discipline technology past a certain tier.
    /// </summary>
    [AutoNetworkedField]
    [DataField("Path", customTypeSerializer: typeof(PrototypeIdSerializer<PathPrototype>))]
    public string? Path;

    [AutoNetworkedField]
    [DataField("currentResearch")]
    public List<string> CurrentResearch = new();

    /// <summary>
    /// Which research disciplines are able to be unlocked
    /// </summary>
    [AutoNetworkedField]
    [DataField("supportedDisciplines", customTypeSerializer: typeof(PrototypeIdListSerializer<PathPrototype>))]
    public List<string> SupportedDisciplines = new();

    /// <summary>
    /// The ids of all the technologies which have been unlocked.
    /// </summary>
    [AutoNetworkedField]
    [DataField("unlockedReasearchs", customTypeSerializer: typeof(PrototypeIdListSerializer<HereticResearchPrototype>))]
    public List<string> UnlockedReasearchs = new();

    /// <summary>
    /// The ids of all the lathe recipes which have been unlocked.
    /// This is maintained alongside the TechnologyIds
    /// </summary>
    /// todo: if you unlock all the recipes in a tech, it doesn't count as unlocking the tech. sadge
    [AutoNetworkedField]
    [DataField("unlockedRecipes", customTypeSerializer: typeof(PrototypeIdListSerializer<UnlockResearchPrototype>))]
    public List<string> UnlockedRecipes = new();
}

/// <summary>
/// Event raised on the database whenever its
/// technologies or recipes are modified.
/// </summary>
/// <remarks>
/// This event is forwarded from the
/// server to all of it's clients.
/// </remarks>
[ByRefEvent]
public readonly record struct TechnologyDatabaseModifiedEvent;
