using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GravityDefenders
{
    public class MetaProgressionUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text permanentCurrencyText;
        [SerializeField] private Transform upgradeButtonParent;
        [SerializeField] private GameObject permanentUpgradeButtonPrefab;

        [System.Serializable]
        private class UpgradeChosenUnityEvent : UnityEvent<PermanentUpgradeData> { }

        [SerializeField] private UpgradeChosenUnityEvent upgradeChosenEvent = new UpgradeChosenUnityEvent();
        [SerializeField] private UnityEvent panelShownEvent = new UnityEvent();
        [SerializeField] private UnityEvent panelHiddenEvent = new UnityEvent();

        public UnityEvent<PermanentUpgradeData> UpgradeChosenEvent => upgradeChosenEvent;
        public UnityEvent PanelShownEvent => panelShownEvent;
        public UnityEvent PanelHiddenEvent => panelHiddenEvent;

    private readonly List<PermanentUpgradeButton> buttonPool = new List<PermanentUpgradeButton>();
    private readonly Dictionary<PermanentUpgradeData, PermanentUpgradeButton> buttonLookup = new Dictionary<PermanentUpgradeData, PermanentUpgradeButton>();
    private readonly HashSet<PermanentUpgradeData> purchasedCache = new HashSet<PermanentUpgradeData>();

        void Awake()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

    public void ShowPanel(List<PermanentUpgradeData> upgrades, HashSet<PermanentUpgradeData> purchased, int currentCurrency)
        {
            if (panel == null || permanentUpgradeButtonPrefab == null || upgradeButtonParent == null)
            {
                Debug.LogWarning("MetaProgressionUI: Missing required references.");
                return;
            }

            panel.SetActive(true);
            panelShownEvent.Invoke();
            EnsureButtonPool(upgrades.Count);
            buttonLookup.Clear();
            purchasedCache.Clear();
            if (purchased != null)
            {
                foreach (PermanentUpgradeData upgrade in purchased)
                {
                    if (upgrade != null)
                    {
                        purchasedCache.Add(upgrade);
                    }
                }
            }

            for (int i = 0; i < buttonPool.Count; i++)
            {
                bool active = i < upgrades.Count;
                buttonPool[i].gameObject.SetActive(active);

                if (!active) continue;

                PermanentUpgradeData upgrade = upgrades[i];
                bool isPurchased = purchasedCache.Contains(upgrade);
                bool canAfford = !isPurchased && currentCurrency >= upgrade.permanentCurrencyCost;

                buttonPool[i].Configure(upgrade, isPurchased, canAfford, HandleUpgradeClicked);
                buttonLookup[upgrade] = buttonPool[i];
            }

            UpdateCurrency(currentCurrency);
        }

        public void HidePanel()
        {
            if (panel == null) return;
            panel.SetActive(false);
            panelHiddenEvent.Invoke();
        }

        public void UpdateCurrency(int amount)
        {
            if (permanentCurrencyText != null)
            {
                permanentCurrencyText.text = $"Permanent Currency: {amount}";
            }

            foreach (KeyValuePair<PermanentUpgradeData, PermanentUpgradeButton> pair in buttonLookup)
            {
                PermanentUpgradeData upgrade = pair.Key;
                bool alreadyPurchased = purchasedCache.Contains(upgrade);
                pair.Value.RefreshState(alreadyPurchased, !alreadyPurchased && amount >= upgrade.permanentCurrencyCost);
            }
        }

        public void RefreshUpgrade(PermanentUpgradeData upgrade, bool purchased, int currentCurrency)
        {
            if (upgrade == null) return;
            if (purchased)
            {
                purchasedCache.Add(upgrade);
            }
            else
            {
                purchasedCache.Remove(upgrade);
            }

            if (buttonLookup.TryGetValue(upgrade, out PermanentUpgradeButton button))
            {
                button.RefreshState(purchased, !purchased && currentCurrency >= upgrade.permanentCurrencyCost);
            }
        }

        private void EnsureButtonPool(int desiredCount)
        {
            while (buttonPool.Count < desiredCount)
            {
                GameObject instance = Instantiate(permanentUpgradeButtonPrefab, upgradeButtonParent);
                PermanentUpgradeButton button = instance.GetComponent<PermanentUpgradeButton>();
                if (button == null)
                {
                    Debug.LogError("MetaProgressionUI: PermanentUpgradeButton prefab missing component.");
                    Destroy(instance);
                    break;
                }

                buttonPool.Add(button);
            }
        }

        private void HandleUpgradeClicked(PermanentUpgradeData upgrade)
        {
            if (upgrade == null) return;
            upgradeChosenEvent.Invoke(upgrade);
        }
    }
}
