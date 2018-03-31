using Harmony;

namespace HungerRevamped {
	internal class StatusBarPatches {

		[HarmonyPatch(typeof(StatusBar), "Awake")]
		private static class SetRedLevel {
			private static void Postfix(StatusBar __instance) {
				if (__instance.m_StatusBarType == StatusBar.StatusBarType.Hunger) {
					__instance.m_FillValueThreshold = Tuning.hungerLevelMalnourished;

					// Ugly way of making the right status bar (always visible / tab controlled) show up at the right time
					if (__instance.m_FillValueRangeActive.x == 0.1f) {
						__instance.m_FillValueRangeActive.x = Tuning.hungerLevelStarving;
					} else if (__instance.m_FillValueRangeActive.y == 0.1f) {
						__instance.m_FillValueRangeActive.y = Tuning.hungerLevelStarving;
					}
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

		[HarmonyPatch(typeof(StatusBar), "GetRateOfChangeHunger")]
		private static class FixHungerStatusArrows {
			private static bool Prefix(ref float __result) {
				HungerRevamped hungerRevamped = HungerRevamped.Instance;
				Hunger hunger = hungerRevamped.hunger;

				float hungerFraction = hunger.m_CurrentReserveCalories / hunger.m_MaxReserveCalories;
				if (hungerFraction < 0.005f || hungerFraction > 0.995f) {
					__result = 0f;
				} else {
					if (hunger.IsAddingCaloriesOverTime()) {
						__result = -hunger.GetCaloriesToAddOverTime();
					} else {
						__result = hunger.GetCurrentCalorieBurnPerHour() + hungerRevamped.GetStoredCaloriesChangePerHour();
					}
				}

				return false;
			}
		}
	}
}
