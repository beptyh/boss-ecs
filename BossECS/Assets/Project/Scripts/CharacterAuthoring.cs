using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using Common;

[RequireComponent(typeof(GhostAuthoringComponent))] // Обязателен для Ghost
public class CharacterAuthoring : MonoBehaviour
{
    class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CharacterTag>(entity);
            AddComponent<NewCharacterTag>(entity);
            AddComponent<Team>(entity);
            AddComponent(entity, new CharacterMoveSpeed { Value = 5f });
            AddBuffer<CharacterMoveInput>(entity);

        }
    }
}
