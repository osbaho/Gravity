using System;
using UnityEngine;
using UnityEngine.UI;

namespace GravityDefenders
{
    public class PermanentUpgradeButton : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text costText;
        [SerializeField] private Button button;

    private PermanentUpgradeData currentUpgrade;
    private Action<PermanentUpgradeData> clickCallback;
        private bool isPurchased;
        private bool canAfford;

    public void Configure(PermanentUpgradeData upgrade, bool purchased, bool affordStatus, Action<PermanentUpgradeData> onClicked)
        {
            currentUpgrade = upgrade;
            isPurchased = purchased;
            canAfford = affordStatus;
            clickCallback = onClicked;

            nameText.text = upgrade.upgradeName;
            descriptionText.text = upgrade.description;
            costText.text = purchased ? "Purchased" : upgrade.permanentCurrencyCost.ToString();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);

            RefreshInteractable();
        }

        public void RefreshState(bool purchased, bool affordStatus)
        {
            isPurchased = purchased;
            canAfford = affordStatus;
            costText.text = isPurchased ? "Purchased" : currentUpgrade.permanentCurrencyCost.ToString();
            RefreshInteractable();
        }

        private void OnButtonClick()
        {
            if (isPurchased) return;
            clickCallback?.Invoke(currentUpgrade);
        }

        private void RefreshInteractable()
        {
            if (isPurchased)
            {
                button.interactable = false;
                return;
            }

            button.interactable = canAfford;
        }
    }
}
