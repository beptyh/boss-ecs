using Common;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct CharacterMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CharacterMoveSpeed>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        new MoveJob
        {
            DeltaTime = deltaTime,
            Tick = tick
        }.Schedule();
    }

    // Сам job
    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;
        public NetworkTick Tick;

        public void Execute(
                    DynamicBuffer<CharacterMoveInput> inputBuffer,
                    ref LocalTransform transform,
                    in CharacterMoveSpeed speed,
                    in PredictedGhost predicted
                                                )
        {
            if (!inputBuffer.GetDataAtTick(Tick, out CharacterMoveInput input))
                return;

            float2 move = input.Value;
            if (math.lengthsq(move) > 1f)
                move = math.normalize(move);

            float3 moveDir = new float3(move.x, 0f, move.y);
            transform.Position += moveDir * speed.Value * DeltaTime;
        }
    }
}
