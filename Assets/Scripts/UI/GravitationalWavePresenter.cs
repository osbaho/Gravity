using UnityEngine;

namespace GravityDefenders
{
    public class GravitationalWavePresenter : MonoBehaviour
    {
        [SerializeField] private GravitationalWaveHUD hud;

        void Awake()
        {
            if (hud == null)
            {
                Debug.LogWarning("GravitationalWavePresenter: HUD reference missing.");
            }
        }

        public void OnTimerTick(float secondsRemaining)
        {
            hud?.UpdateTimer(secondsRemaining);
        }

        public void OnWaveTriggered()
        {
            hud?.ShowWaveTriggered();
        }

        public void OnRunStarted()
        {
            hud?.ResetStatus();
        }
    }
}
