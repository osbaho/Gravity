using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class TurretBuilder : MonoBehaviour
    {
        public static TurretBuilder Instance { get; private set; }

        [Header("Events")]
        [SerializeField] private UnityEvent<TurretBlueprint, TurretBuildSlot> turretBuiltEvent = new UnityEvent<TurretBlueprint, TurretBuildSlot>();
        [SerializeField] private UnityEvent<TurretBlueprint, TurretBuildSlot> turretBuildFailedEvent = new UnityEvent<TurretBlueprint, TurretBuildSlot>();

        public UnityEvent<TurretBlueprint, TurretBuildSlot> TurretBuiltEvent => turretBuiltEvent;
        public UnityEvent<TurretBlueprint, TurretBuildSlot> TurretBuildFailedEvent => turretBuildFailedEvent;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public bool CanAfford(TurretBlueprint blueprint)
        {
            if (blueprint == null || ResourceManager.Instance == null) return false;

            TurretCost cost = blueprint.cost;
            return ResourceManager.Instance.HasEnoughResources(cost.PrimaryResource, cost.MiningResourceA, cost.MiningResourceB);
        }

    public bool TryBuildTurret(TurretBlueprint blueprint, TurretBuildSlot slot)
        {
            if (blueprint == null || slot == null)
            {
                turretBuildFailedEvent.Invoke(blueprint, slot);
                return false;
            }

            if (slot.HasTurret)
            {
                Debug.Log("TurretBuilder: Slot already occupied.");
                turretBuildFailedEvent.Invoke(blueprint, slot);
                return false;
            }

            if (blueprint.archetype == null)
            {
                Debug.LogError("TurretBuilder: Blueprint missing archetype reference.");
                turretBuildFailedEvent.Invoke(blueprint, slot);
                return false;
            }

            TurretCost cost = blueprint.cost;
            if (ResourceManager.Instance == null || !ResourceManager.Instance.HasEnoughResources(cost.PrimaryResource, cost.MiningResourceA, cost.MiningResourceB))
            {
                Debug.Log("Not enough resources to build turret.");
                turretBuildFailedEvent.Invoke(blueprint, slot);
                return false;
            }

            ResourceManager.Instance.SpendResources(cost.PrimaryResource, cost.MiningResourceA, cost.MiningResourceB);
            Vector3 placementPosition = slot.GetPlacementPosition();
            Quaternion placementRotation = slot.GetPlacementRotation();
            Turret placedTurret = SpawnTurretForBlueprint(blueprint, placementPosition, placementRotation, slot.transform);

            if (placedTurret == null)
            {
                Debug.LogError("TurretBuilder: Failed to spawn turret for blueprint.");
                turretBuildFailedEvent.Invoke(blueprint, slot);
                return false;
            }

            placedTurret.Initialize(blueprint.archetype);
            slot.AssignTurret(placedTurret, blueprint);
            turretBuiltEvent.Invoke(blueprint, slot);
            return true;
        }

        private Turret SpawnTurretForBlueprint(TurretBlueprint blueprint, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject instance = null;
            Turret turretComponent = null;

            if (blueprint.customPrefab != null)
            {
                instance = Instantiate(blueprint.customPrefab, position, rotation, parent);
                turretComponent = instance.GetComponent<Turret>();

                if (turretComponent == null)
                {
                    Debug.LogError("TurretBuilder: Custom prefab does not include a Turret component.");
                    Destroy(instance);
                    return null;
                }
            }
            else
            {
                string turretName = string.IsNullOrWhiteSpace(blueprint.turretName) ? "Turret" : blueprint.turretName;
                instance = new GameObject($"Turret_{turretName}");
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.transform.SetParent(parent, true);
                turretComponent = instance.AddComponent<Turret>();

                if (blueprint.visualPrefab != null)
                {
                    GameObject visual = Instantiate(blueprint.visualPrefab, instance.transform);
                    visual.transform.localPosition = blueprint.visualOffset;
                    visual.transform.localRotation = Quaternion.identity;
                    visual.transform.localScale = Vector3.one;
                }
            }

            return turretComponent;
        }
    }
}
