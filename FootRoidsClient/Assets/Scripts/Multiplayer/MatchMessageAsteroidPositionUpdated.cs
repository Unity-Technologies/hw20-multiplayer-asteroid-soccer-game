using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MatchMessageAsteroidPositionUpdated : MatchMessage<MatchMessageAsteroidPositionUpdated>
    {
        public readonly int id;
        public readonly float x;
        public readonly float y;

        public MatchMessageAsteroidPositionUpdated(int id, float _x, float _y)
        {
            this.id = id;
            this.x = _x;
            this.y = _y;
        }
    }
}

