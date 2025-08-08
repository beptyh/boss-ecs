using UnityEngine;

namespace Common
{
    public enum TeamType: byte
    {
        None = 0,
        Team1 = 1,
        Team2 = 2,
        AutoAsign = byte.MaxValue
    }
}