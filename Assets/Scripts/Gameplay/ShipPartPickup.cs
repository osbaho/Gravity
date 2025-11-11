using UnityEngine;

namespace GravityDefenders
{
    [RequireComponent(typeof(Collider))]
    public class ShipPartPickup : MonoBehaviour
    {
        [Header("Presentation")]
        [SerializeField] private float rotationSpeed = 45f;
        [SerializeField] private ParticleSystem collectEffect;

        [Header("Collection")]
        [SerializeField] private LayerMask collectorLayers = ~0;

        private bool collected;

        void Awake()
        {
            Collider pickupCollider = GetComponent<Collider>();
            if (pickupCollider != null)
            {
                pickupCollider.isTrigger = true;
            }
        }

        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (collected) return;
            if (!IsCollectorLayer(other.gameObject.layer)) return;

            Collect();
        }

        void OnMouseDown()
        {
            Collect();
        }

        private bool IsCollectorLayer(int layer)
        {
            return (collectorLayers.value & (1 << layer)) != 0;
        }

        private void Collect()
        {
            if (collected) return;

            GameManager manager = GameManager.Instance;
            if (manager == null) return;

            if (!manager.RegisterShipPartCollected())
            {
                return;
            }

            collected = true;

            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            WaveManager.Instance?.NotifyShipPartPickupRemoved();
            Destroy(gameObject);
        }

        void OnDisable()
        {
            if (!collected)
            {
                WaveManager.Instance?.NotifyShipPartPickupRemoved();
            }
        }
    }
}
