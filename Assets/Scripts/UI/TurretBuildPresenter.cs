using UnityEngine;

namespace GravityDefenders
{
    public class TurretBuildPresenter : MonoBehaviour
    {
        [SerializeField] private TurretBuildMenu buildMenu;
        [SerializeField] private TurretBuilder builder;

        private TurretBuildSlot currentSlot;

        void Awake()
        {
            if (buildMenu == null)
            {
                Debug.LogWarning("TurretBuildPresenter: BuildMenu reference missing.");
            }

            if (builder == null)
            {
                builder = TurretBuilder.Instance;
            }
        }

        public void OnSlotSelected(TurretBuildSlot slot)
        {
            currentSlot = slot;
            buildMenu?.OpenMenu(slot);
        }

        public void OnBlueprintSelected(TurretBlueprint blueprint)
        {
            if (builder == null || currentSlot == null)
            {
                Debug.LogWarning("TurretBuildPresenter: Missing builder or slot during selection.");
                return;
            }

            bool success = builder.TryBuildTurret(blueprint, currentSlot);
            if (success)
            {
                buildMenu?.CloseMenu();
                currentSlot = null;
            }
        }

        public void OnTurretBuilt(TurretBlueprint blueprint, TurretBuildSlot slot)
        {
            if (slot == currentSlot)
            {
                buildMenu?.CloseMenu();
                currentSlot = null;
            }
        }

        public void OnBuildFailed(TurretBlueprint blueprint, TurretBuildSlot slot)
        {
            if (slot == currentSlot)
            {
                // Keep menu open for another selection attempt
            }
        }

        public void DeselectSlot()
        {
            currentSlot = null;
            buildMenu?.CloseMenu();
        }
    }
}
