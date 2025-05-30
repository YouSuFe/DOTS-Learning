using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitMoverAuthoring : MonoBehaviour
{
    public float value;
    public float rotationSpeed;

    public class Baker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover
            {
                value = authoring.value,
                rotationSpeed = authoring.rotationSpeed,
            });
        }
    }
}

public struct UnitMover : IComponentData
{
    public float value;
    public float rotationSpeed;
    public float3 targetPosition;
}
