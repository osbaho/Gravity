using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GravityDefenders
{
    public class ShipStatusHUD : MonoBehaviour
    {
        [Header("Health Display")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text healthLabel;

        [Header("Ship Parts Display")]
        [SerializeField] private TMP_Text shipPartLabel;
        [SerializeField] private string shipPartFormat = "Parts: {0}/{1}";

        public void SetHealth(int current, int max)
        {
            max = Mathf.Max(1, max);
            current = Mathf.Clamp(current, 0, max);

            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthLabel != null)
            {
                healthLabel.text = $"{current}/{max}";
            }
        }

        public void SetShipPartsProgress(int collected, int required)
        {
            if (shipPartLabel == null) return;
            shipPartLabel.text = string.Format(shipPartFormat, Mathf.Max(0, collected), Mathf.Max(1, required));
        }
    }
}
