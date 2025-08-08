using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Common
{
    public struct CharacterTag : IComponentData { }
    public struct NewCharacterTag : IComponentData { }
    public struct OwnerCharacterTag : IComponentData { }
    
    [GhostComponent(PrefabType = GhostPrefabType.All)]
    public struct Team: IComponentData
    {
        [GhostField] public TeamType Value;
    }
    
    [GhostComponent(PrefabType = GhostPrefabType.All)]
    public struct CharacterMoveSpeed : IComponentData
    {
        [GhostField(Quantization = 1000)] public float Value;
    }

 



}
