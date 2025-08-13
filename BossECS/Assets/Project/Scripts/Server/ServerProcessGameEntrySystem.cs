using Assets.Project.Scripts;
using Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerProcessGameEntrySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePrefabs>();
        var builder = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<GameTeamRequest, ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var characterPrefab = SystemAPI.GetSingleton<GamePrefabs>().Character;

        foreach (var (teamRequest, requestSource, requestEntity) in SystemAPI.Query<GameTeamRequest, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            ecb.DestroyEntity(requestEntity);
            ecb.AddComponent<NetworkStreamInGame>(requestSource.SourceConnection);

            var requestedTeamType = teamRequest.Value;
            if (requestedTeamType == TeamType.AutoAsign)
            {
                requestedTeamType = TeamType.Team1;
            }

            var clientId = SystemAPI.GetComponent<NetworkId>(requestSource.SourceConnection).Value;
            var newPlayer = ecb.Instantiate(characterPrefab);
            var spawnPosition = new float3(0f, 1f, 0f);
            var newTransform = LocalTransform.FromPosition(spawnPosition);
            ecb.SetComponent(newPlayer, newTransform);
            ecb.SetComponent(newPlayer, new GhostOwner { NetworkId = clientId });
            ecb.SetComponent(newPlayer, new Team { Value = requestedTeamType });

            ecb.AppendToBuffer(requestSource.SourceConnection, new LinkedEntityGroup { Value = newPlayer });
            ecb.SetComponent(requestSource.SourceConnection, new CommandTarget
            {
                targetEntity = newPlayer
            });


        }
        ecb.Playback(state.EntityManager);
    }

}
