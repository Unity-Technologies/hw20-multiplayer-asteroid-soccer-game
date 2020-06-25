using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MatchMessageAsteroidSpawned : MatchMessage<MatchMessageAsteroidSpawned>
    {
        public readonly int ElementId;
        public readonly Vector3 ElementPosition;
        public readonly Vector3 ElementDirection;

        public MatchMessageAsteroidSpawned(Vector3 pos, Vector3 dir, int elementId)
        {
            ElementId = elementId;
            ElementPosition = pos;
            ElementDirection = dir;
        }
    }
}
