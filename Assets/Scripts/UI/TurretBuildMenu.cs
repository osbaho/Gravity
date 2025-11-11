using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class TurretBuildMenu : MonoBehaviour
    {
        [Header("Blueprints")]
        [SerializeField] private List<TurretBlueprint> availableBlueprints = new List<TurretBlueprint>();

        [System.Serializable]
        private class BlueprintUnityEvent : UnityEvent<TurretBlueprint> { }

        [SerializeField] private BlueprintUnityEvent blueprintSelectedEvent = new BlueprintUnityEvent();
        [SerializeField] private UnityEvent menuOpenedEvent = new UnityEvent();
        [SerializeField] private UnityEvent menuClosedEvent = new UnityEvent();

        public UnityEvent<TurretBlueprint> BlueprintSelectedEvent => blueprintSelectedEvent;
        public UnityEvent MenuOpenedEvent => menuOpenedEvent;
        public UnityEvent MenuClosedEvent => menuClosedEvent;

        private TurretBuildSlot currentSlot;

        public IReadOnlyList<TurretBlueprint> GetBlueprints() => availableBlueprints;

        public void OpenMenu(TurretBuildSlot slot)
        {
            currentSlot = slot;
            gameObject.SetActive(true);
            menuOpenedEvent.Invoke();
        }

        public void CloseMenu()
        {
            currentSlot = null;
            gameObject.SetActive(false);
            menuClosedEvent.Invoke();
        }

        public void SelectBlueprint(TurretBlueprint blueprint)
        {
            if (blueprint == null)
            {
                Debug.LogWarning("TurretBuildMenu: Attempted to select null blueprint.");
                return;
            }

            if (currentSlot == null)
            {
                Debug.LogWarning("TurretBuildMenu: No active slot when selecting blueprint.");
                return;
            }

            blueprintSelectedEvent.Invoke(blueprint);
        }

        public TurretBuildSlot GetCurrentSlot() => currentSlot;
    }
}
