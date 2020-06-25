namespace Multiplayer
{
    public class MatchMessageRotationUpdated : MatchMessage<MatchMessageRotationUpdated>
    {
        public float rot;

        public MatchMessageRotationUpdated(float rot)
        {
            this.rot = rot;
        }
    }
}