namespace Multiplayer
{
    public class MatchMessageSpawnShip : MatchMessage<MatchMessageSpawnShip>
    {
        public readonly bool team;
        public readonly int playerIndex;
        public readonly int elementId;
        public readonly float x;
        public readonly float y;
        public readonly float angle;

        public MatchMessageSpawnShip(string id, bool team, int playerIndex, float _x, float _y, float _a)
        {
            this.team = team;
            this.playerIndex = playerIndex;
            elementId = id.GetHashCode();
            x = _x;
            y = _y;
            angle = _a;
        }
    }
}