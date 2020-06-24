namespace Multiplayer
{
    public class MatchMessagePositionUpdated : MatchMessage<MatchMessagePositionUpdated>
    {
        public float posX;
        public float posY;

        public MatchMessagePositionUpdated(float posX, float posY)
        {
            this.posX = posX;
            this.posY = posY;
        }
    }

}