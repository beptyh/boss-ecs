using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerMoveInputSystem : SystemBase
{
    private MovementActions _input;

    protected override void OnCreate()
    {
        _input = new MovementActions();

        // ⛔️ Обновление только если CommandTarget и NetworkId существуют
        RequireForUpdate<CommandTarget>();
        RequireForUpdate<NetworkId>();
    }

    protected override void OnStartRunning()
    {
        _input.Enable();
    }

    protected override void OnStopRunning()
    {
        _input.Disable();
    }

    protected override void OnUpdate()
    {
        var inputVector = _input.BaseMap.PlayerMovement.ReadValue<Vector2>();
        if (inputVector.sqrMagnitude > 1f)
            inputVector.Normalize();

        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        var commandTarget = SystemAPI.GetSingletonRW<CommandTarget>();
        var targetEntity = commandTarget.ValueRO.targetEntity;

        if (targetEntity == Entity.Null)
            return;

        var inputBuffer = SystemAPI.GetBuffer<CharacterMoveInput>(targetEntity);

        // 🟢 Не дублируй input для одного и того же тика!
        if (inputBuffer.Length == 0 || inputBuffer[inputBuffer.Length - 1].Tick != tick)
        {
            inputBuffer.Add(new CharacterMoveInput
            {
                Tick = tick,
                Value = inputVector
            });
        }
    }

}
