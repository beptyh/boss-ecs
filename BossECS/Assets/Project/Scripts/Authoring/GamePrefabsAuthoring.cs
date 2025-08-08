
using Unity.Entities;
using UnityEngine;

namespace Assets.Project.Scripts
{
    public class GamePrefabsAuthoring: MonoBehaviour
    {
        public GameObject Character;

        public class GamePrefabsBaker : Baker<GamePrefabsAuthoring>
        {
            public override void Bake(GamePrefabsAuthoring authoring)
            {
                var prefabContainerEntity = GetEntity(TransformUsageFlags.None);
                AddComponent(prefabContainerEntity, new Common.GamePrefabs
                {
                    Character = GetEntity(authoring.Character, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
 
}
