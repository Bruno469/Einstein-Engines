using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.PsycUpdate;

/// <summary>
/// A prototype that defines a set of items and visuals in a specific starter set for the antagonist thief
/// </summary>
[Prototype("UpdateCristalSet")]
public sealed partial class UpdateCristalSetPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [DataField] public string Name { get; private set; } = string.Empty;
    [DataField] public string Description { get; private set; } = string.Empty;
    [DataField] public SpriteSpecifier Sprite { get; private set; } = SpriteSpecifier.Invalid;

    [DataField] public List<EntProtoId> Content = new();
}
