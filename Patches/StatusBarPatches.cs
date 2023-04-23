using HarmonyLib;
using Il2Cpp;

namespace HungerRevamped {
	internal static class StatusBarPatches {

		[HarmonyPatch(typeof(GenericStatusBarSpawner), "Awake")]
		private static class SetRedLevelAndSprites {
			private static void Postfix(GenericStatusBarSpawner __instance) {
				StatusBar statusBar = __instance.m_SpawnedObject.GetComponent<StatusBar>();
				if (statusBar.m_StatusBarType != StatusBar.StatusBarType.Hunger) return;

				statusBar.m_ThresholdCritical = Tuning.hungerLevelStarvingWarning;

				UISprite buff = statusBar.m_BuffObject.GetComponent<UISprite>();
				buff.color = new UnityEngine.Color(0f, 0.429f, 0.276f); // Same green as carry weight buff

				UISprite debuff = statusBar.m_DebuffObject.GetComponent<UISprite>();
				debuff.spriteName = "inv_statBack";
				debuff.height = (int) (buff.height * 0.2);
				debuff.width = (int) (buff.width * 0.45);
				debuff.transform.localPosition = buff.transform.localPosition;
				debuff.color = new UnityEngine.Color(0.64f, 0.2f, 0.23f); // m_StatusMainSpriteBelowThresholdColor
			}
		}

		[HarmonyPatch(typeof(StatusBar), "Update")]
		private static class SetStatusBarVisuals {
			private static void Postfix(StatusBar __instance) {
				if (__instance.m_StatusBarType != StatusBar.StatusBarType.Hunger) return;

				float fillValue = __instance.GetFillValue();
				double storedCalories = HungerRevamped.Instance.storedCalories;

				// Transferring calories out of calorie store
				Utils.SetActive(__instance.m_DebuffObject, fillValue < Tuning.hungerLevelMalnourished && storedCalories > 0);
				// Transferring calories into calorie store
				Utils.SetActive(__instance.m_BuffObject, fillValue > Tuning.hungerLevelWellFed && storedCalories < Tuning.maximumStoredCalories);

				// Starving hunger bar colors
				if (fillValue < Tuning.hungerLevelStarving) {
					__instance.m_OuterBoxSprite.color = InterfaceManager.GetInstance().m_StatusOuterBoxEmptyColor;
                    Utils.SetActive(__instance.m_SpriteWhenEmpty.gameObject, true);
					__instance.SetActiveBacksplash(__instance.m_BacksplashDepleted);
				}
			}
		}

		[HarmonyPatch(typeof(StatusBar), "GetRateOfChange")]
		private static class FixHungerStatusArrows {
			private static bool Prefix(StatusBar __instance, ref float __result) {
				if (__instance.m_StatusBarType != StatusBar.StatusBarType.Hunger) return true;

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
