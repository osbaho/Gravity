using UnityEngine;

namespace GravityDefenders
{
    [RequireComponent(typeof(Health))]
    public class CentralShip : MonoBehaviour
    {
        private Health health;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        // You can add specific logic for the ship here if needed in the future.
    }
}
