using UnityEngine;

namespace GravityDefenders
{
    public class GravitationalWavePanelPresenter : MonoBehaviour
    {
        [SerializeField] private GravitationalWavePanel panel;

        void Awake()
        {
            if (panel == null)
            {
                Debug.LogWarning("GravitationalWavePanelPresenter: Panel reference missing.");
            }
        }

        public void OnTimerTick(float secondsRemaining)
        {
            panel?.UpdateTimer(secondsRemaining);
        }

        public void OnWaveTriggered()
        {
            panel?.ShowImpact();
        }

        public void OnRunStarted()
        {
            panel?.ResetStatus();
        }
    }
}
