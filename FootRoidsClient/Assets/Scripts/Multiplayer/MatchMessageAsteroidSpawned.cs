using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MatchMessageAsteroidSpawned : MatchMessage<MatchMessageAsteroidSpawned>
    {
        public readonly int ElementId;
        public readonly float x;
        public readonly float y;


        public MatchMessageAsteroidSpawned(float _x, float _y, int elementId)
        {
            ElementId = elementId;

            x = _x;
            y = _y;
        }
    }
}
