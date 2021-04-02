using Harmony;

namespace HungerRevamped {
	internal static class GameStatePatches {

		internal static bool IsApplyingDeferredFoodPoisoning = false;

		[HarmonyPatch(typeof(Hunger), "Start")]
		private static class HungerStart {
			private static void Prefix(Hunger __instance) {
				if (__instance.m_StartHasBeenCalled)
					return;

				float burnRateScale = GameManager.GetExperienceModeManagerComponent().GetCalorieBurnScale();
				__instance.m_MaxReserveCalories = Tuning.maximumHungerCalories * burnRateScale;
				__instance.m_StarvingCalorieThreshold = Tuning.hungerLevelStarving * __instance.m_MaxReserveCalories;

				HungerRevamped.Instance = new HungerRevamped(__instance);
			}
		}

		[HarmonyPatch(typeof(Condition), "Start")]
		private static class ConditionStart {
			private static void Prefix(Condition __instance) {
				if (__instance.m_StartHasBeenCalled)
					return;

				__instance.m_HPDecreasePerDayFromStarving = 0f;
			}
		}

		[HarmonyPatch(typeof(WellFed), "Start")]
		private static class WellFedStart {
			private static void Prefix(WellFed __instance) {
				if (__instance.m_StartHasBeenCalled)
					return;

				__instance.m_MaxConditionBonusPercent = 0f;
			}
		}

		[HarmonyPatch(typeof(Hunger), "Update")]
		private static class UpdateHungerRevampedAfterHunger {
			private static void Postfix() {
				HungerRevamped.Instance.Update();
			}
		}

		[HarmonyPatch(typeof(Freezing), "CalculateBodyTemperature")]
		private static class ApplyStoredCaloriesWarmthModifier {
			private static void Postfix(ref float __result) {
				__result += HungerRevamped.Instance.GetStoredCaloriesWarmthBonus();
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "CalculateModifiedCalorieBurnRate")]
		private static class ModifyCalorieConsumption {
			private static void Postfix(ref float __result) {
				__result *= HungerRevamped.Instance.GetCalorieBurnRateMultiplier();
			}
		}

		[HarmonyPatch(typeof(FoodPoisoning), "FoodPoisoningStart")]
		private static class HandleDeferredFoodPoisoning {

			// Defer food poisoning when acquired by eating food
			private static bool Prefix(string causeId, ref bool __state) {
				if (MenuSettings.settings.realisticFoodPoisoningChance && Watch_OnEatingComplete.isExecuting) {
					return false; // do nothing if executed before the postfix for OnEatingComplete with modified food poisoning chances on
								  // need to wait for the postfix to decide if the player will get food poisoning
				} else if (IsApplyingDeferredFoodPoisoning || causeId == "GAMEPLAY_TaintedFood")
				{
					return true; // either a console command or applying the deferred food poisoning
				} else {
					HungerRevamped.Instance.AddFoodPoisoningCall(causeId);
					return false; // shh, later
				}
			}

			// Wake the player up when applying deferred food poisoning while asleep
			private static void Postfix(ref bool __state) {
				if (GameManager.GetPlayerManagerComponent().PlayerIsDead() || InterfaceManager.m_Panel_ChallengeComplete.IsEnabled())
					return;
				if (GameManager.InCustomMode() && !GameManager.GetCustomMode().m_EnableFoodPoisoning)
					return;

				Rest rest = GameManager.GetRestComponent();
				if (rest.IsSleeping() && IsApplyingDeferredFoodPoisoning) {
					rest.EndSleeping(true);
				}
			}
		}

		[HarmonyPatch(typeof(PlayerManager), "FirstAidConsumed")]
		private static class ClearDeferredFoodPoisoningsWhenConsumingAntibiotics {
			private static void Postfix(GearItem gi) {
				if (gi && gi.m_FirstAidItem && gi.m_FirstAidItem.m_ProvidesAntibiotics) {
					HungerRevamped.Instance.OnPlayerTookAntibiotics();
				}
			}
		}

		[HarmonyPatch(typeof(WellFed), "Update")]
		private static class WellFedNewUpdate {
			private static bool Prefix(WellFed __instance) {
				if (GameManager.m_IsPaused)
					return false;

				bool active = __instance.HasWellFed();
				float carryBonus = HungerRevamped.Instance.GetCarryBonus();
				__instance.m_CarryCapacityBonusKG = carryBonus;

				if (!active && carryBonus >= Tuning.wellFedCarryBonusStart) {
					__instance.WellFedStart(__instance.GetCauseLocalizationId(), true, false);
				} else if (active && carryBonus < Tuning.wellFedCarryBonusEnd) {
					__instance.WellFedEnd();
				}
				return false;
			}
		}
	}
}
