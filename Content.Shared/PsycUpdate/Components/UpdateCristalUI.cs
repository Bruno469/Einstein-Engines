using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.PsycUpdate;

[Serializable, NetSerializable]
public sealed class UpdateCristalBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly Dictionary<int, UpdateCristalSetInfo> Sets;
    public int MaxSelectedSets;

    public UpdateCristalBoundUserInterfaceState(Dictionary<int, UpdateCristalSetInfo> sets, int max)
    {
        Sets = sets;
        MaxSelectedSets = max;
    }
}

[Serializable, NetSerializable]
public sealed class UpdateCristalChangeSetMessage : BoundUserInterfaceMessage
{
    public readonly int SetNumber;

    public UpdateCristalChangeSetMessage(int setNumber)
    {
        SetNumber = setNumber;
    }
}

[Serializable, NetSerializable]
public sealed class UpdateCristalApproveMessage : BoundUserInterfaceMessage
{
    public UpdateCristalApproveMessage() { }
}

[Serializable, NetSerializable]
public enum UpdateCristalUIKey : byte
{
    Key
};

[Serializable, NetSerializable, DataDefinition]
public partial struct UpdateCristalSetInfo
{
    [DataField]
    public string Name;

    [DataField]
    public string Description;

    [DataField]
    public SpriteSpecifier Sprite;

    public bool Selected;

    public UpdateCristalSetInfo(string name, string desc, SpriteSpecifier sprite, bool selected)
    {
        Name = name;
        Description = desc;
        Sprite = sprite;
        Selected = selected;
    }
}
