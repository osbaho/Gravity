using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    [RequireComponent(typeof(Collider))]
    public class TurretBuildSlot : MonoBehaviour
    {
        [Header("Placement")] 
        [SerializeField] private Transform placementPoint;
        [SerializeField] private bool allowMouseInteraction = true;

        [Header("Events")]
        [SerializeField] private UnityEvent<TurretBuildSlot> slotSelectedEvent = new UnityEvent<TurretBuildSlot>();
        [SerializeField] private UnityEvent<TurretBuildSlot, Turret> turretPlacedEvent = new UnityEvent<TurretBuildSlot, Turret>();
        [SerializeField] private UnityEvent<TurretBuildSlot> turretClearedEvent = new UnityEvent<TurretBuildSlot>();

        public UnityEvent<TurretBuildSlot> SlotSelectedEvent => slotSelectedEvent;
        public UnityEvent<TurretBuildSlot, Turret> TurretPlacedEvent => turretPlacedEvent;
        public UnityEvent<TurretBuildSlot> TurretClearedEvent => turretClearedEvent;

        public bool HasTurret => placedTurret != null;
        public Turret CurrentTurret => placedTurret;
        public TurretBlueprint CurrentBlueprint { get; private set; }

        private Turret placedTurret;

        public Vector3 GetPlacementPosition()
        {
            return placementPoint != null ? placementPoint.position : transform.position;
        }

        public Quaternion GetPlacementRotation()
        {
            return placementPoint != null ? placementPoint.rotation : transform.rotation;
        }

        public void AssignTurret(Turret turret, TurretBlueprint blueprint)
        {
            placedTurret = turret;
            CurrentBlueprint = blueprint;
            turretPlacedEvent.Invoke(this, turret);
        }

        public void ClearSlot()
        {
            if (placedTurret != null)
            {
                Destroy(placedTurret.gameObject);
                placedTurret = null;
                CurrentBlueprint = null;
                turretClearedEvent.Invoke(this);
            }
        }

        void OnMouseDown()
        {
            if (!allowMouseInteraction) return;
            slotSelectedEvent.Invoke(this);
        }
    }
}
