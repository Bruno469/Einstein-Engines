using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Chaplain;

[Serializable, NetSerializable]
public sealed class NullRodBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly Dictionary<int, NullRodSetInfo> Sets;
    public int MaxSelectedSets;

    public NullRodBoundUserInterfaceState(Dictionary<int, NullRodSetInfo> sets, int max)
    {
        Sets = sets;
        MaxSelectedSets = max;
    }
}

[Serializable, NetSerializable]
public sealed class NullRodChangeSetMessage : BoundUserInterfaceMessage
{
    public readonly int SetNumber;

    public NullRodChangeSetMessage(int setNumber)
    {
        SetNumber = setNumber;
    }
}

[Serializable, NetSerializable]
public sealed class NullRodApproveMessage : BoundUserInterfaceMessage
{
    public NullRodApproveMessage() { }
}

[Serializable, NetSerializable]
public enum NullRodUIKey : byte
{
    Key
};

[Serializable, NetSerializable, DataDefinition]
public partial struct NullRodSetInfo
{
    [DataField]
    public string Name;

    [DataField]
    public string Description;

    [DataField]
    public SpriteSpecifier Sprite;

    public bool Selected;

    public NullRodSetInfo(string name, string desc, SpriteSpecifier sprite, bool selected)
    {
        Name = name;
        Description = desc;
        Sprite = sprite;
        Selected = selected;
    }
}
