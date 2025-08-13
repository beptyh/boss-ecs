using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Скрипт вешается на обычную камеру.
/// Камера будет следовать за вашим ECS-игроком, как только он появится на клиенте.
/// </summary>
public class CameraAutoFollowEcsPlayer : MonoBehaviour
{
    public Vector3 cameraOffset = new Vector3(0, 10, -10); // смещение относительно игрока
    public World ecsWorld; // Можно назначить в инспекторе, или оставить пустым для авто-детекта

    private EntityManager em;
    private Entity playerEntity = Entity.Null;

    void Start()
    {
        if (ecsWorld == null)
            ecsWorld = World.DefaultGameObjectInjectionWorld;
        em = ecsWorld.EntityManager;
    }

    void LateUpdate()
    {
        // Если игрок уже найден — следим
        if (playerEntity != Entity.Null && em.Exists(playerEntity))
        {
            var t = em.GetComponentData<LocalTransform>(playerEntity);
            transform.position = (Vector3)t.Position + cameraOffset;
            // Если хочешь, чтобы камера всегда смотрела на игрока:
            // transform.LookAt((Vector3)t.Position);
            return;
        }

        // Если игрок еще не найден — ищем его среди сущностей
        var myNetworkId = GetLocalNetworkId();
        if (myNetworkId == 0) return; // еще не получили NetworkId

        // Вариант через GhostOwner (самый универсальный способ для NetCode)
        using (var query = em.CreateEntityQuery(typeof(GhostOwner), typeof(LocalTransform)))
        {
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var entity in entities)
            {
                var owner = em.GetComponentData<GhostOwner>(entity);
                if (owner.NetworkId == myNetworkId)
                {
                    playerEntity = entity;
                    Debug.Log("Camera теперь следует за сущностью " + entity);
                    break;
                }
            }
        }
    }

    ushort GetLocalNetworkId()
    {
        var q = em.CreateEntityQuery(typeof(NetworkId));
        if (q.CalculateEntityCount() == 0) return 0;
        return (ushort)q.GetSingleton<NetworkId>().Value;
    }
}
