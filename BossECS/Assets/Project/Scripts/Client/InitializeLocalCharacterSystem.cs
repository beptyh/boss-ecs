using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Transforms;
using Common;

namespace Client
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct InitializeLocalCharacterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var commandTargetEntity = SystemAPI.GetSingletonEntity<CommandTarget>();
            var commandTarget = SystemAPI.GetComponentRW<CommandTarget>(commandTargetEntity);

            foreach (var (transform, entity) in SystemAPI.Query<LocalTransform>()
                .WithAll<GhostOwnerIsLocal>()
                .WithNone<OwnerCharacterTag>()
                .WithEntityAccess())
            {
                // Назначаем управление на этого игрока
                commandTarget.ValueRW.targetEntity = entity;

                ecb.AddComponent<OwnerCharacterTag>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}