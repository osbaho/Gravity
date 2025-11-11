using TMPro;
using UnityEngine;

namespace GravityDefenders
{
    public class GravitationalWaveHUD : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private TMP_Text statusLabel;

        [Header("Formatting")]
        [SerializeField] private string timerFormat = "Wave in {0:0}s";
        [SerializeField] private string impactMessage = "Gravitational Wave Impact!";
        [SerializeField] private float statusDisplayDuration = 3f;

        private float statusTimer;

        void Update()
        {
            if (statusLabel == null) return;
            if (statusTimer <= 0f) return;

            statusTimer -= Time.unscaledDeltaTime;
            if (statusTimer <= 0f)
            {
                statusLabel.text = string.Empty;
            }
        }

        public void UpdateTimer(float secondsRemaining)
        {
            if (timerLabel == null) return;
            timerLabel.text = string.Format(timerFormat, Mathf.Max(0f, secondsRemaining));
        }

        public void ShowWaveTriggered()
        {
            if (statusLabel != null)
            {
                statusLabel.text = impactMessage;
                statusTimer = statusDisplayDuration;
            }
        }

        public void ResetStatus()
        {
            statusTimer = 0f;
            if (statusLabel != null)
            {
                statusLabel.text = string.Empty;
            }
        }
    }
}
