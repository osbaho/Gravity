using UnityEngine;

namespace GravityDefenders
{
    public class UpgradePanelPresenter : MonoBehaviour
    {
        [SerializeField] private UpgradePanelUI panel;

        public void DisplayOptions(UpgradeData[] options)
        {
            if (panel == null)
            {
                Debug.LogWarning("UpgradePanelPresenter: Panel reference missing.");
                return;
            }

            panel.ShowUpgrades(options);
        }

        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogWarning("UpgradePanelPresenter: Received null upgrade selection.");
                return;
            }

            UpgradeManager.Instance?.ApplyUpgrade(upgrade);
        }
    }
}
