using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((
            RefRO<LocalTransform> localTransform,
            RefRW<MeleeAttack> meleeAttack,
            RefRO<Target> target,
            RefRW<UnitMover> unitMover)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<MeleeAttack>,
                RefRO<Target>,
                RefRW<UnitMover>>())
        {
            if(target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            float meleeAttackDistanceSq = 2f;
            if(math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.Position) > meleeAttackDistanceSq)
            {
                // Target is too far
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
            }
            else
            {
                // Target is close enugh to attack
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;

                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if(meleeAttack.ValueRO.timer > 0)
                {
                    continue;
                }

                meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);

                targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChanged = true;

            }
        }
    }


}
