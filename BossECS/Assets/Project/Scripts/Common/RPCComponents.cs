using System;
using Unity.NetCode;

namespace Common
{
    public struct GameTeamRequest : IRpcCommand
    {
        public TeamType Value;
    }
 
}