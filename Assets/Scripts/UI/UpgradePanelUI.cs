using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class UpgradePanelUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeButton[] upgradeButtons;

        [System.Serializable]
        private class UpgradeSelectedUnityEvent : UnityEvent<UpgradeData> { }

        [SerializeField] private UpgradeSelectedUnityEvent upgradeSelectedEvent = new UpgradeSelectedUnityEvent();

        public UnityEvent<UpgradeData> UpgradeSelectedEvent => upgradeSelectedEvent;

        void Awake()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void ShowUpgrades(UpgradeData[] upgrades)
        {
            if (upgrades == null)
            {
                ShowUpgradesInternal(System.Array.Empty<UpgradeData>());
            }
            else
            {
                ShowUpgradesInternal(upgrades);
            }
        }

        public void ShowUpgrades(List<UpgradeData> upgrades)
        {
            if (upgrades == null)
            {
                ShowUpgradesInternal(System.Array.Empty<UpgradeData>());
            }
            else
            {
                ShowUpgradesInternal(upgrades.ToArray());
            }
        }

        private void ShowUpgradesInternal(UpgradeData[] upgrades)
        {
            if (panel == null)
            {
                Debug.LogWarning("UpgradePanelUI: Panel reference missing.");
                return;
            }

            panel.SetActive(true);
            Time.timeScale = 0f;

            int availableButtons = upgradeButtons != null ? upgradeButtons.Length : 0;

            for (int i = 0; i < availableButtons; i++)
            {
                if (i < upgrades.Length)
                {
                    if (upgradeButtons[i] != null)
                    {
                        upgradeButtons[i].SetUpgrade(upgrades[i], OnUpgradeSelected);
                        upgradeButtons[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (upgradeButtons[i] != null)
                    {
                        upgradeButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnUpgradeSelected(UpgradeData selectedUpgrade)
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
            upgradeSelectedEvent.Invoke(selectedUpgrade);
        }
    }
}
