using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct CharacterMoveInput : ICommandData
{
    public NetworkTick Tick { get; set; }
    public float2 Value;
}
