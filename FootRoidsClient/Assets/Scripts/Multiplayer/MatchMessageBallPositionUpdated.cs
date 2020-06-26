namespace Multiplayer
{
    public class MatchMessageBallPositionUpdated : MatchMessage<MatchMessageBallPositionUpdated>
    {
        public readonly int id;
        public readonly float posX;
        public readonly float posY;
        public readonly float angle;

        public MatchMessageBallPositionUpdated(int id, float posX, float posY, float angle)
        {
            this.id = id;
            this.posX = posX;
            this.posY = posY;
            this.angle = angle;
        }
    }
}