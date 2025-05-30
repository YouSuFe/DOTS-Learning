using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial struct HealthDeadTestSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRO<Health> health, Entity entity) in SystemAPI.Query<RefRO<Health>>().WithEntityAccess())
        {
            if(health.ValueRO.healthAmount <= 0)
            {
                //entityCommandBuffer.DestroyEntity(entity);
                entityCommandBuffer.DestroyEntity(entity);
            }
        }

        //entityCommandBuffer.Playback(state.EntityManager);
    }


}
