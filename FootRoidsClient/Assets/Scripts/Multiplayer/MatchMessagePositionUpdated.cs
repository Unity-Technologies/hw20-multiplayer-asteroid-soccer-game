namespace Multiplayer
{
    public class MatchMessagePositionUpdated : MatchMessage<MatchMessagePositionUpdated>
    {
        public readonly int id;
        public readonly float posX;
        public readonly float posY;
        public readonly float angle;

        public MatchMessagePositionUpdated(string id, float posX, float posY, float angle)
        {
            this.id = id.GetHashCode();
            this.posX = posX;
            this.posY = posY;
            this.angle = angle;
        }
    }

}