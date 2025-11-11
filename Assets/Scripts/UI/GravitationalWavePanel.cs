using TMPro;
using UnityEngine;

namespace GravityDefenders
{
    public class GravitationalWavePanel : MonoBehaviour
    {
        [Header("Display Elements")]
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private TMP_Text statusLabel;

        [Header("Formatting")]
        [SerializeField] private string timerFormat = "Wave in {0:0}s";
        [SerializeField] private string impactText = "Wave Impact!";
        [SerializeField] private float impactDisplayTime = 3f;

        private float impactTimer;

        void OnEnable()
        {
            ResetStatus();
        }

        void Update()
        {
            if (impactTimer <= 0f) return;
            impactTimer -= Time.unscaledDeltaTime;
            if (impactTimer <= 0f && statusLabel != null)
            {
                statusLabel.text = string.Empty;
            }
        }

        public void UpdateTimer(float secondsRemaining)
        {
            if (timerLabel == null) return;
            timerLabel.text = string.Format(timerFormat, Mathf.Max(0f, secondsRemaining));
        }

        public void ShowImpact()
        {
            if (statusLabel == null) return;
            statusLabel.text = impactText;
            impactTimer = impactDisplayTime;
        }

        public void ResetStatus()
        {
            impactTimer = 0f;
            if (statusLabel != null)
            {
                statusLabel.text = string.Empty;
            }
        }
    }
}
