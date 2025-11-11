using UnityEngine;

namespace GravityDefenders
{
    [CreateAssetMenu(fileName = "Turret Blueprint", menuName = "Gravity Defenders/Turret Blueprint")]
    public class TurretBlueprint : ScriptableObject
    {
        [Header("Display")]
        public string turretName;
        [TextArea]
        public string description;
        public Sprite icon;

        [Header("Setup")]
        public TurretArchetype archetype;
        [Tooltip("Optional prefab that already contains a Turret component.")]
        public GameObject customPrefab;
        [Tooltip("Optional visual that will be instantiated as a child when custom prefab is omitted.")]
        public GameObject visualPrefab;
        public Vector3 visualOffset;
        public TurretCost cost;
    }
}
