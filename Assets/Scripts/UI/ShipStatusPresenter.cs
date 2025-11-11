using UnityEngine;

namespace GravityDefenders
{
    public class ShipStatusPresenter : MonoBehaviour
    {
        [SerializeField] private ShipStatusHUD hud;

        public void OnHealthChanged(int current, int max)
        {
            if (hud == null)
            {
                Debug.LogWarning("ShipStatusPresenter: HUD reference missing.");
                return;
            }

            hud.SetHealth(current, max);
        }

        public void OnShipPartsProgress(int collected, int required)
        {
            if (hud == null)
            {
                Debug.LogWarning("ShipStatusPresenter: HUD reference missing.");
                return;
            }

            hud.SetShipPartsProgress(collected, required);
        }
    }
}
