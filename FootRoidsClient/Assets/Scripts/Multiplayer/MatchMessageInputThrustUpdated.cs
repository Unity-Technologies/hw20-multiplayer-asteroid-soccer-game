namespace Multiplayer
{
    public class MatchMessageInputThrustUpdated : MatchMessage<MatchMessageInputThrustUpdated>
    {
        public readonly int id;
        public readonly float input;

        public MatchMessageInputThrustUpdated(string id, float input)
        {
            this.id = id.GetHashCode();
            this.input = input;
        }
    }
}