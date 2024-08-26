using Robust.Shared.Serialization;

namespace Content.Shared.Heretic.Components
{
    [NetSerializable, Serializable]
    public enum ResearchUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class ConsoleUnlockResearchMessage : BoundUserInterfaceMessage
    {
        public string Id;

        public ConsoleUnlockResearchMessage(string id)
        {
            Id = id;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ResearchBoundInterfaceState : BoundUserInterfaceState
    {
        public int Points;
        public ResearchBoundInterfaceState(int points)
        {
            Points = points;
        }
    }
}
