using UnityEngine;

namespace GravityDefenders
{
    public class MapZoneSelectionPresenter : MonoBehaviour
    {
        [SerializeField] private MapZoneSelectionPanel panel;

        private MapZone currentZone;

        void Awake()
        {
            if (panel == null)
            {
                Debug.LogWarning("MapZoneSelectionPresenter: Panel reference missing.");
            }
        }

        public void OnZoneClicked(MapZone zone)
        {
            currentZone = zone;
            panel?.Show(zone);
        }

        public void OnShieldStateChanged(MapZone zone)
        {
            if (zone == currentZone)
            {
                panel?.Refresh(zone);
            }
        }

        public void OnShieldExpansionResult(MapZone zone)
        {
            if (zone == currentZone)
            {
                panel?.Refresh(zone);
            }
        }

        public void ClosePanel()
        {
            panel?.Hide();
            currentZone = null;
        }
    }
}
