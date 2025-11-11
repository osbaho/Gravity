using UnityEngine;

namespace GravityDefenders
{
    public class RunFlowPresenter : MonoBehaviour
    {
        [SerializeField] private RunFlowPanel panel;

        void Awake()
        {
            if (panel == null)
            {
                Debug.LogWarning("RunFlowPresenter: Panel reference missing.");
            }
        }

        public void OnRunStarted()
        {
            panel?.ShowRunStart();
        }

        public void OnHideRunStart()
        {
            panel?.HideRunStart();
        }

        public void OnGameWon()
        {
            panel?.ShowVictory();
        }

        public void OnGameLost()
        {
            panel?.ShowDefeat();
        }
    }
}
