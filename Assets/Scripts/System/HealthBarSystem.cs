using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    // Due to using Camera logic which is external call, burst compile cannot be working.
    // So, we disabled it. Monkey said in Job, it can be handled but I have no idea how.
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Vector3 cameraForward = Vector3.zero;
        if(Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
        }

        foreach((
            RefRW<LocalTransform> localTransform,
            RefRO<HealthBar> healthBar)
            in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRO<HealthBar>>())
        {
            // To get world rotation of the local bar visual object to make it look at the camera
            LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);
            if(localTransform.ValueRO.Scale == 1f)
            {
                localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
            }

            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);

            if(!health.onHealthChanged)
            {
                continue;
            }

            Debug.Log("Health visual update");

            float healthNormalized = (float)health.healthAmount / health.healthAmountMax;

            // To hide the bar if health is full
            if(healthNormalized == 1f)
            {
                localTransform.ValueRW.Scale = 0f;
            }
            else
            {
                localTransform.ValueRW.Scale = 1f;
            }

            RefRW<PostTransformMatrix> barVisualPostTransformMatrix =
                SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
            barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);

            //RefRW<LocalTransform> barVisualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(healthBar.ValueRO.barVisualEntity);
            //barVisualLocalTransform.ValueRW.Scale = healthNormalized;
        }
    }


}
