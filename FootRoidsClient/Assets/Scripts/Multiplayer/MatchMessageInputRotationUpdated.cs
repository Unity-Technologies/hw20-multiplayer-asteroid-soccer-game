namespace Multiplayer
{
    public class MatchMessageInputRotationUpdated : MatchMessage<MatchMessageInputRotationUpdated>
    {
        public readonly int id;
        public readonly float input;

        public MatchMessageInputRotationUpdated(string id, float input)
        {
            this.id = id.GetHashCode();
            this.input = input;
        }
    }
}