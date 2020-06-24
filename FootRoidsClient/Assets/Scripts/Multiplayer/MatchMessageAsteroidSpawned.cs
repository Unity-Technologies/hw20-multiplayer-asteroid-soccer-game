using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MatchMessageAsteroidSpawned : MatchMessage<MatchMessageAsteroidSpawned>
    {
        public readonly int ElementId;
        public readonly Transform ElementTransform;

        public MatchMessageAsteroidSpawned(Transform elementTransform, int elementId)
        {
            ElementId = elementId;
            ElementTransform = elementTransform;
        }
    }
}
