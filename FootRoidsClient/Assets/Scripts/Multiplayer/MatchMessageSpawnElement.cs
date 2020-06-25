using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MatchMessageSpawnElement : MatchMessage<MatchMessageSpawnElement>
    {
        public readonly int ElementId;
        public readonly float x;
        public readonly float y;
        public readonly float angle;

        public MatchMessageSpawnElement(int id, float _x, float _y, float _a)
        {
            ElementId = id;
            x = _x;
            y = _y;
            angle = _a;
        }
    }
}

