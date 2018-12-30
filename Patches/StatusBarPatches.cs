using System.Reflection;
using Harmony;

namespace HungerRevamped {
	internal static class StatusBarPatches {

		[HarmonyPatch(typeof(GenericStatusBarSpawner), "Awake")]
		private static class SetRedLevel {
			private static void Postfix(GenericStatusBarSpawner __instance) {
				StatusBar statusBar = __instance.m_SpawnedObject.GetComponent<StatusBar>();
				if (statusBar.m_StatusBarType == StatusBar.StatusBarType.Hunger) {
					statusBar.m_ThresholdCritical = Tuning.hungerLevelMalnourished;
				}
			}
		}

		[HarmonyPatch(typeof(StatusBar), "SetSpriteColors")]
		private static class SetStarvingSpriteColor {
			private static void Postfix(StatusBar __instance, float fillValue) {
				if (__instance.m_StatusBarType == StatusBar.StatusBarType.Hunger && fillValue > 0f && fillValue <= Tuning.hungerLevelStarving) {
					__instance.m_OuterBoxSprite.color = GameManager.GetInterfaceManager().m_StatusOuterBoxEmptyColor;
				}
			}
		}

		[HarmonyPatch(typeof(StatusBar), "UpdateBacksplash")]
		private static class SetStarvingVisuals {
			private static readonly MethodInfo setActiveBacksplash = AccessTools.Method(typeof(StatusBar), "SetActiveBacksplash");

			private static void Postfix(StatusBar __instance, float fillValue) {
				if (__instance.m_StatusBarType == StatusBar.StatusBarType.Hunger && fillValue > 0f && fillValue <= Tuning.hungerLevelStarving) {
					Utils.SetActive(__instance.m_SpriteWhenEmpty.gameObject, true);
					setActiveBacksplash.Invoke(__instance, new[] { __instance.m_BacksplashDepleted });
				}
			}
		}

		[HarmonyPatch(typeof(StatusBar), "GetRateOfChangeHunger")]
		private static class FixHungerStatusArrows {
			private static bool Prefix(ref float __result) {
				HungerRevamped hungerRevamped = HungerRevamped.Instance;
				Hunger hunger = hungerRevamped.hunger;

				float hungerFraction = hunger.m_CurrentReserveCalories / hunger.m_MaxReserveCalories;
				if (hungerFraction < 0.005f || hungerFraction > 0.999f) {
					__result = 0f;
				} else {
					if (hunger.IsAddingCaloriesOverTime()) {
						__result = -hunger.GetCaloriesToAddOverTime();
					} else {
						__result = 0.8f * (hunger.GetCurrentCalorieBurnPerHour() + hungerRevamped.GetStoredCaloriesChangePerHour());
					}
				}

				return false;
			}
		}
	}
}
