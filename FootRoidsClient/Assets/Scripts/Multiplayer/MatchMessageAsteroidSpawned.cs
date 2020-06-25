using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MatchMessageAsteroidSpawned : MatchMessage<MatchMessageAsteroidSpawned>
    {
        public readonly int ElementId;
        public readonly float position_x;
        public readonly float position_y;
        public readonly float position_z;

        public readonly float direction_x;
        public readonly float direction_y;
        public readonly float direction_z;


        public MatchMessageAsteroidSpawned(Vector3 pos, Vector3 dir, int elementId)
        {
            ElementId = elementId;

            // hax b/c json serialize...
            position_x = pos.x;
            position_y = pos.y;
            position_z = pos.z;

            direction_x = dir.x;
            direction_y = dir.y;
            direction_z = dir.z;
        }
    }
}
