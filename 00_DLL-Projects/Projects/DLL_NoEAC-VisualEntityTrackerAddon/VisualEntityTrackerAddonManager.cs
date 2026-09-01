using System.Collections;
using UnityEngine;

namespace VisualEntityTrackerAddon
{
    public class VisualEntityTrackerAddonManager : MonoBehaviour
    {
        public static VisualEntityTrackerAddonManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public static void ScheduleApply(int entityId)
        {
            if (entityId < 0)
            {
                return;
            }

            if (!VisualEntityTrackerPresence.IsPresent())
            {
                return;
            }

            if (Instance != null)
            {
                Instance.StartCoroutine(Instance.ApplyRoutine(entityId));
                return;
            }

            VisualEntityTrackerAddonService.ApplySavedMode(entityId);
        }

        private IEnumerator ApplyRoutine(int entityId)
        {
            VisualEntityTrackerAddonService.ApplySavedMode(entityId);
            yield return new WaitForSeconds(0.25f);
            VisualEntityTrackerAddonService.ApplySavedMode(entityId);
            yield return new WaitForSeconds(1f);
            VisualEntityTrackerAddonService.ApplySavedMode(entityId);
        }
    }
}
