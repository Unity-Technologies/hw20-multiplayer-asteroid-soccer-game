using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MatchMessageSpawnElement : MatchMessage<MatchMessageSpawnElement>
    {
        public readonly int elementId;
        public readonly float x;
        public readonly float y;
        public readonly float angle;

        public MatchMessageSpawnElement(string id, float _x, float _y, float _a)
        {
            elementId = id.GetHashCode();
            x = _x;
            y = _y;
            angle = _a;
        }
    }
}

