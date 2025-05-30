using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{

    public static UnitSelectionManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Action OnSelectionAreaStart;
    public Action OnSelectionAreaEnd;

    private Vector2 selectionStartMousePosition;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;

            OnSelectionAreaStart?.Invoke();
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 selectionEndMousePosition = Input.mousePosition;


            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            for(int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
            }


            entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

            entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<LocalTransform> localTransfromArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            Rect selectionAreaRect = GetSelectionAreaRect();

            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMinimun = 40f;
            bool isMultiplerSelection = selectionAreaSize > multipleSelectionSizeMinimun;

            if (isMultiplerSelection)
            {
                for (int i = 0; i < localTransfromArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = localTransfromArray[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);

                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                    }
                }
            }
            else
            {
                // Same as : new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>().Build(entityManager);
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();

                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                int unitsLayer = 6;
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << unitsLayer,
                        GroupIndex = 0,
                    }
                };
                if(collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if(entityManager.HasComponent<Unit>(raycastHit.Entity))
                    {
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                    }
                }

            }

            OnSelectionAreaEnd?.Invoke();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            for(int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = mouseWorldPosition;
                unitMoverArray[i] = unitMover;
            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;
        Vector2 lowerLeftCorner = new Vector2(
            System.Math.Min(selectionStartMousePosition.x, selectionEndMousePosition.x),
            System.Math.Min(selectionStartMousePosition.y, selectionEndMousePosition.y)
            );

        Vector2 upperRightCorner = new Vector2(
            System.Math.Max(selectionStartMousePosition.x, selectionEndMousePosition.x),
            System.Math.Max(selectionStartMousePosition.y, selectionEndMousePosition.y)
            );
        return new Rect(
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y
            );
    }
}

