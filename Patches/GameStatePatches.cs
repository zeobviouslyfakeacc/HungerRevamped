using Harmony;
using System.Collections.Generic;
using System.Reflection;

namespace HungerRevamped {
	internal class GameStatePatches {

		[HarmonyPatch(typeof(Hunger), "Start")]
		private static class HungerStart {
			private static void Prefix(Hunger __instance, bool __state) {
				if ((bool) AccessTools.Field(typeof(Hunger), "m_StartHasBeenCalled").GetValue(__instance))
					return;

				SetMaxHungerBarCalories(__instance);

				HungerRevamped.Instance = new HungerRevamped(__instance);
			}
		}

		[HarmonyPatch(typeof(CustomExperienceMode), "UpdateBaseValues")]
		private static class CustomExperienceModeDoneLoading {
			private static void Postfix() {
				if (!HungerRevamped.HasInstance()) {
					// If this condition is true, Hinterland finally fixed their load order and this patch isn't needed anymore.
					return;
				}

				SetMaxHungerBarCalories(HungerRevamped.Instance.hunger);
			}
		}

		private static void SetMaxHungerBarCalories(Hunger hunger) {
			float burnRateScale = GameManager.GetExperienceModeManagerComponent().GetCalorieBurnScale();
			hunger.m_MaxReserveCalories = Tuning.maximumHungerCalories * burnRateScale;
			hunger.m_StarvingCalorieThreshold = Tuning.hungerLevelStarving * hunger.m_MaxReserveCalories;
		}

		[HarmonyPatch(typeof(Condition), "Start")]
		private static class ConditionStart {
			private static void Prefix(Condition __instance, bool __state) {
				if ((bool) AccessTools.Field(typeof(Condition), "m_StartHasBeenCalled").GetValue(__instance))
					return;

				__instance.m_HPDecreasePerDayFromStarving = 0f;
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

		[HarmonyPatch(typeof(PlayerManager), "OnEatingComplete")]
		private static class RedirectCallsToFoodPoisoningStart {
			private static readonly MethodInfo from = AccessTools.Method(typeof(FoodPoisoning), "FoodPoisoningStart");
			private static readonly MethodInfo to = AccessTools.Method(typeof(RedirectCallsToFoodPoisoningStart), "Target");

			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				return Transpilers.MethodReplacer(instructions, from, to);
			}

			private static void Target(FoodPoisoning fp, string causeId, bool displayIcon, bool nofx) {
				// Unused method arguments required to clear the argument stack
				HungerRevamped.Instance.AddFoodPoisoningCall(causeId);
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
	}
}
