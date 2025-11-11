using UnityEngine;

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
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                TryMineAtMousePosition();
            }
        }

        private void TryMineAtMousePosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
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
                        Debug.Log($"Mined {minedAmount} of type {vein.resourceType}");
                        // TODO: Add a visual effect at the hit point (e.g., sparks)
                    }
                }
            }
        }
    }
}
