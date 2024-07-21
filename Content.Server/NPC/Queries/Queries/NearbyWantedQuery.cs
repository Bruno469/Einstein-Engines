using Content.Server.NPC.Queries.Queries;
using Robust.Shared.Serialization;

namespace Content.Server.NPC.Queries;

/// <summary>
/// Query to find nearby entities that are marked as "wanted" by security.
/// </summary>
[DataDefinition]
public sealed partial class NearbyWantedQuery : UtilityQuery
{
}
