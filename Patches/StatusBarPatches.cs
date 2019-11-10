using System.Reflection;
using Harmony;

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

				// The buff object has a nice "plus" sprite, but there's no minus sprite...
				// So let's create our own by cutting off 27 pixels from the top and the bottom of that plus sprite
				UISpriteData debuffData = new UISpriteData();
				debuffData.CopyFrom(buff.GetAtlasSprite());
				debuffData.name = "ico_Status_BuffMinus";
				debuffData.paddingTop = 27;
				debuffData.paddingBottom = 27;
				debuffData.y += debuffData.paddingTop;
				debuffData.height -= debuffData.paddingTop + debuffData.paddingBottom;
				buff.atlas.spriteList.Add(debuffData);
				buff.atlas.MarkSpriteListAsChanged();

				UISprite debuff = statusBar.m_DebuffObject.GetComponent<UISprite>();
				debuff.spriteName = "ico_Status_BuffMinus";
				debuff.height = buff.height;
				debuff.width = buff.width;
				debuff.transform.localPosition = buff.transform.localPosition;
				debuff.color = new UnityEngine.Color(0.64f, 0.2f, 0.23f); // m_StatusMainSpriteBelowThresholdColor
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

		[HarmonyPatch(typeof(StatusBar), "UpdateBacksplash")]
		private static class ShowCalorieStoreSymbols {
			private static void Postfix(StatusBar __instance, float fillValue) {
				if (__instance.m_StatusBarType == StatusBar.StatusBarType.Hunger) {
					double storedCalories = HungerRevamped.Instance.storedCalories;
					Utils.SetActive(__instance.m_DebuffObject, fillValue <= Tuning.hungerLevelMalnourished && storedCalories > 0);
					Utils.SetActive(__instance.m_BuffObject, fillValue > Tuning.hungerLevelWellFed && storedCalories < Tuning.maximumStoredCalories);
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
