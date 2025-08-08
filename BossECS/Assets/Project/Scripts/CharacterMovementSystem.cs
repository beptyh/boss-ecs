using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Common;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct CharacterMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CharacterMoveSpeed>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach (var (inputBuffer, transform, speed, predicted, entity) in
            SystemAPI.Query<DynamicBuffer<CharacterMoveInput>, RefRW<LocalTransform>, RefRO<CharacterMoveSpeed>, RefRO<PredictedGhost>>()
            .WithEntityAccess())
        {
            if (inputBuffer.Length == 0)
                continue;

            // Бери input строго для текущего tick!
            var input = inputBuffer.GetDataAtTick(tick, out var found);
            if (!found)
                continue;

            float2 move = input.Value;
            if (math.lengthsq(move) > 1f)
                move = math.normalize(move);

            float3 moveDir = new float3(move.x, 0f, move.y); // XY, если Z-вперёд
            transform.ValueRW.Position += moveDir * speed.ValueRO.Value * deltaTime;
        }
    }
}
