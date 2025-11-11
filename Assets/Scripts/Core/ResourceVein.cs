using UnityEngine;

namespace GravityDefenders
{
    public enum MiningResourceType { A, B }

    public class ResourceVein : MonoBehaviour
    {
        [Header("Vein Settings")]
        public MiningResourceType resourceType;
        public int totalResources = 1000;

        public int Mine(int amount)
        {
            int minedAmount = Mathf.Min(amount, totalResources);
            totalResources -= minedAmount;

            if (totalResources <= 0)
            {
                // TODO: Add a visual effect for the vein being depleted
                Debug.Log($"Resource vein depleted.");
                Destroy(gameObject);
            }

            return minedAmount;
        }
    }
}
