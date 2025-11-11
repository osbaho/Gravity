using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class MapZoneSelectionPanel : MonoBehaviour
    {
        [Header("Display Elements")]
        [SerializeField] private TMP_Text zoneNameLabel;
        [SerializeField] private TMP_Text shieldStatusLabel;
        [SerializeField] private TMP_Text costLabel;

        [Header("Formatting")]
        [SerializeField] private string zoneNameFormat = "Zone: {0}";
        [SerializeField] private string shieldedText = "Shielded";
        [SerializeField] private string unshieldedText = "Unshielded";
        [SerializeField] private string costFormat = "Cost: {0}P / {1}A / {2}B";

        [Header("Events")]
        [SerializeField] private UnityEvent onPanelShown = new UnityEvent();
        [SerializeField] private UnityEvent onPanelHidden = new UnityEvent();

        public UnityEvent PanelShown => onPanelShown;
        public UnityEvent PanelHidden => onPanelHidden;

        private MapZone currentZone;

        void Awake()
        {
            Hide();
        }

        public void Show(MapZone zone)
        {
            if (zone == null)
            {
                Hide();
                return;
            }

            currentZone = zone;
            gameObject.SetActive(true);
            Refresh(zone);
            onPanelShown.Invoke();
        }

        public void Hide()
        {
            currentZone = null;
            gameObject.SetActive(false);
            onPanelHidden.Invoke();
        }

        public void Refresh(MapZone zone)
        {
            if (zone == null) return;

            if (zoneNameLabel != null)
            {
                zoneNameLabel.text = string.Format(zoneNameFormat, zone.name);
            }

            if (shieldStatusLabel != null)
            {
                shieldStatusLabel.text = zone.IsShielded ? shieldedText : unshieldedText;
            }

            if (costLabel != null)
            {
                TurretCost cost = MapManager.Instance != null ? MapManager.Instance.ShieldExpansionCost : default;
                costLabel.text = string.Format(costFormat, cost.PrimaryResource, cost.MiningResourceA, cost.MiningResourceB);
            }
        }

        public void RequestShieldExpansion()
        {
            currentZone?.RequestShieldExpansion();
        }
    }
}
