using System;
using UnityEngine;
using UnityEngine.UI;

namespace GravityDefenders
{
    public class UpgradeButton : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Button button;

        private UpgradeData currentUpgrade;
    private Action<UpgradeData> onUpgradeSelected;

    public void SetUpgrade(UpgradeData upgrade, Action<UpgradeData> selectionCallback)
        {
            currentUpgrade = upgrade;
            onUpgradeSelected = selectionCallback;

            nameText.text = upgrade.upgradeName;
            descriptionText.text = upgrade.description;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            onUpgradeSelected?.Invoke(currentUpgrade);
        }
    }
}
