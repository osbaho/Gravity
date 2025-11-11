using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class RunFlowPanel : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject runStartPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        [Header("Events")]
        [SerializeField] private UnityEvent onRunStarted = new UnityEvent();
        [SerializeField] private UnityEvent onVictoryShown = new UnityEvent();
        [SerializeField] private UnityEvent onDefeatShown = new UnityEvent();

        public UnityEvent OnRunStarted => onRunStarted;
        public UnityEvent OnVictoryShown => onVictoryShown;
        public UnityEvent OnDefeatShown => onDefeatShown;

        void Awake()
        {
            SetPanel(runStartPanel, false);
            SetPanel(victoryPanel, false);
            SetPanel(defeatPanel, false);
        }

        public void ShowRunStart()
        {
            SetPanel(runStartPanel, true);
            SetPanel(victoryPanel, false);
            SetPanel(defeatPanel, false);
            onRunStarted.Invoke();
        }

        public void HideRunStart()
        {
            SetPanel(runStartPanel, false);
        }

        public void ShowVictory()
        {
            SetPanel(runStartPanel, false);
            SetPanel(defeatPanel, false);
            SetPanel(victoryPanel, true);
            onVictoryShown.Invoke();
        }

        public void ShowDefeat()
        {
            SetPanel(runStartPanel, false);
            SetPanel(victoryPanel, false);
            SetPanel(defeatPanel, true);
            onDefeatShown.Invoke();
        }

        private static void SetPanel(GameObject panel, bool state)
        {
            if (panel != null)
            {
                panel.SetActive(state);
            }
        }
    }
}
