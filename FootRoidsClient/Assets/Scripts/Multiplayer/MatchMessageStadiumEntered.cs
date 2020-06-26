using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class MatchMessageStadiumEntered : MatchMessage<MatchMessageStadiumEntered>
    {
        public readonly string UserId;

        public MatchMessageStadiumEntered(string id)
        {
            UserId = id;
        }
    }
}

