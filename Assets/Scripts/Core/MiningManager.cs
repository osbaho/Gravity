using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GravityDefenders
{
    public class MiningManager : MonoBehaviour
    {
        public static MiningManager Instance { get; private set; }

        [Header("Mining Settings")]
        [SerializeField] private int miningAmountPerClick = 50;
        [SerializeField] private LayerMask miningLayerMask; // Set this to the layer your veins are on

        public static float miningYieldMultiplier = 1f;

        private Camera mainCamera;

        [System.Serializable]
        private class MiningUnityEvent : UnityEvent<int, MiningResourceType> { }

        [Header("Events")]
        [SerializeField] private MiningUnityEvent resourcesMinedEvent = new MiningUnityEvent();
        public UnityEvent<int, MiningResourceType> ResourcesMinedEvent => resourcesMinedEvent;

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
            mainCamera = Camera.main;
        }

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryMineAtMousePosition();
            }
#else
#error This project requires the new Input System (ENABLE_INPUT_SYSTEM). Set Active Input Handling to "Input System".
#endif
        }

        private void TryMineAtMousePosition()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }
#if ENABLE_INPUT_SYSTEM
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            
#endif
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, miningLayerMask))
            {
                ResourceVein vein = hit.collider.GetComponent<ResourceVein>();
                if (vein != null)
                {
                    int minedAmount = vein.Mine(Mathf.RoundToInt(miningAmountPerClick * miningYieldMultiplier));
                    if (minedAmount > 0)
                    {
                        if (vein.resourceType == MiningResourceType.A)
                        {
                            ResourceManager.Instance.AddMiningResources(minedAmount, 0);
                        }
                        else
                        {
                            ResourceManager.Instance.AddMiningResources(0, minedAmount);
                        }
                        resourcesMinedEvent.Invoke(minedAmount, vein.resourceType);
                        Debug.Log($"Mined {minedAmount} of type {vein.resourceType}");
                        // TODO: Add a visual effect at the hit point (e.g., sparks)
                    }
                }
            }
        }
    }
}
