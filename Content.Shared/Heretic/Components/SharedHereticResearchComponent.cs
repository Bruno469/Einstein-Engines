using Robust.Shared.Serialization;

namespace Content.Shared.Heretic.Components
{
    [NetSerializable, Serializable]
    public enum HereticResearchUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class UnlockHereticResearchMessage : BoundUserInterfaceMessage
    {
        public string Id;

        public UnlockHereticResearchMessage(string id)
        {
            Id = id;
        }
    }

    [Serializable, NetSerializable]
    public sealed class HereticResearchBoundInterfaceState : BoundUserInterfaceState
    {
        public int Points;
        public HereticResearchBoundInterfaceState(int points)
        {
            Points = points;
        }
    }
}
