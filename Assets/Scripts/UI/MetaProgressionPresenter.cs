using System.Collections.Generic;
using UnityEngine;

namespace GravityDefenders
{
    public class MetaProgressionPresenter : MonoBehaviour
    {
        [SerializeField] private MetaProgressionUI metaProgressionUI;

        private readonly List<PermanentUpgradeData> upgradeCache = new List<PermanentUpgradeData>();
        private readonly HashSet<PermanentUpgradeData> purchasedCache = new HashSet<PermanentUpgradeData>();

        void Awake()
        {
            if (metaProgressionUI != null)
            {
                metaProgressionUI.UpgradeChosenEvent.AddListener(HandleUpgradeChosen);
            }
        }

        void OnDestroy()
        {
            if (metaProgressionUI != null)
            {
                metaProgressionUI.UpgradeChosenEvent.RemoveListener(HandleUpgradeChosen);
            }
        }

        public void ShowPanel()
        {
            CacheUpgrades();
            RefreshPurchasedCache();
            int currency = ResourceManager.Instance != null ? ResourceManager.Instance.PermanentCurrency : 0;
            metaProgressionUI?.ShowPanel(upgradeCache, purchasedCache, currency);
        }

        public void HidePanel()
        {
            metaProgressionUI?.HidePanel();
        }

        public void OnPermanentCurrencyChanged(int amount)
        {
            metaProgressionUI?.UpdateCurrency(amount);
        }

        public void OnPermanentUpgradePurchased(PermanentUpgradeData upgrade)
        {
            if (upgrade == null) return;
            purchasedCache.Add(upgrade);
            int currency = ResourceManager.Instance != null ? ResourceManager.Instance.PermanentCurrency : 0;
            metaProgressionUI?.RefreshUpgrade(upgrade, true, currency);
            metaProgressionUI?.UpdateCurrency(currency);
        }

        private void HandleUpgradeChosen(PermanentUpgradeData upgrade)
        {
            if (upgrade == null) return;

            MetaProgressionManager manager = MetaProgressionManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("MetaProgressionPresenter: MetaProgressionManager missing.");
                return;
            }

            manager.PurchaseUpgrade(upgrade);
            RefreshPurchasedCache();
            int currency = ResourceManager.Instance != null ? ResourceManager.Instance.PermanentCurrency : 0;
            metaProgressionUI?.RefreshUpgrade(upgrade, manager.IsUpgradePurchased(upgrade), currency);
            metaProgressionUI?.UpdateCurrency(currency);
        }

        private void CacheUpgrades()
        {
            upgradeCache.Clear();
            MetaProgressionManager manager = MetaProgressionManager.Instance;
            if (manager == null) return;

            foreach (PermanentUpgradeData upgrade in manager.GetAllPermanentUpgrades())
            {
                if (upgrade != null)
                {
                    upgradeCache.Add(upgrade);
                }
            }
        }

        private void RefreshPurchasedCache()
        {
            purchasedCache.Clear();
            MetaProgressionManager manager = MetaProgressionManager.Instance;
            if (manager == null) return;

            foreach (PermanentUpgradeData upgrade in manager.GetPurchasedUpgrades())
            {
                if (upgrade != null)
                {
                    purchasedCache.Add(upgrade);
                }
            }
        }
    }
}
