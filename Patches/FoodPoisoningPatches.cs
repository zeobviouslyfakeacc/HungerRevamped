using HarmonyLib;
using UnityEngine;

namespace HungerRevamped {

	/*
	 * For some reason, food items have two cutoff values for food poisoning:
	 *         x > 0.745: No   food poisoning chance
	 * 0.745 > x > 0.200: Low  food poisoning chance
	 * 0.200 > x        : High food poisoning chance
	 *
	 * When knowing how this system works, it ruins the randomness of the food poisoning system.
	 * It's always just "check if below 20%, if yes, throw away".
	 * A 74% condition food item should not have the same food poisoning chance as a 21% condition one.
	 * So let's fix this:
	 */

	[HarmonyPatch(typeof(FoodPoisoning), "FoodPoisoningStart")]
	internal static class FoodPoisoningStartPatch {

		internal static bool disableFoodPoisoning = false;

		// Don't immediately apply food poisoning when acquired by eating food
		private static bool Prefix() {
			return !disableFoodPoisoning;
		}

		// Wake the player up when applying (deferred) food poisoning while asleep
		private static void Postfix() {
			if (GameManager.GetPlayerManagerComponent().PlayerIsDead() || InterfaceManager.m_Panel_ChallengeComplete.IsEnabled()) return;
			if (GameManager.InCustomMode() && !GameManager.GetCustomMode().m_EnableFoodPoisoning) return;
			if (disableFoodPoisoning) return;

			Rest rest = GameManager.GetRestComponent();
			if (rest.IsSleeping()) {
				rest.EndSleeping(true);
			}
			PassTime passTime = GameManager.GetPassTime();
			if (passTime.IsPassingTime()) {
				passTime.End();
			}
		}
	}

	[HarmonyPatch(typeof(PlayerManager), "OnEatingComplete")]
	internal static class FoodPoisoningChanges {

		private static bool ShouldRun() => MenuSettings.settings.delayedFoodPoisoning
				|| MenuSettings.settings.realisticFoodPoisoningChance
				|| MenuSettings.settings.removeFoodPoisonImmunity;

		private static void Prefix() {
			if (!ShouldRun()) return;
			FoodPoisoningStartPatch.disableFoodPoisoning = true;
		}

		private static void Postfix(PlayerManager __instance, float progress) {
			if (!ShouldRun()) return;
			FoodPoisoningStartPatch.disableFoodPoisoning = false;

			// Emulate early return conditions of PlayerManager.EatingComplete_Internal which don't add food poisoning
			GearItem foodItemEaten = __instance.m_FoodItemEaten;
			if (!foodItemEaten) return;
			if (foodItemEaten.m_FoodItem.m_MustConsumeAll && !Utils.Approximately(progress, 1f)) return;

			bool giveFoodPoisoning;
			if (!MenuSettings.settings.removeFoodPoisonImmunity && GameManager.GetSkillCooking().NoParasitesOrFoodPosioning() && !foodItemEaten.m_FoodItem.m_IsRawMeat) {
				giveFoodPoisoning = false;
			} else if (MenuSettings.settings.realisticFoodPoisoningChance) {
				giveFoodPoisoning = CustomRollForFoodPoisoning(foodItemEaten, __instance.m_FoodItemEatenStartingCalories);
			} else {
				giveFoodPoisoning = foodItemEaten.RollForFoodPoisoning(__instance.m_FoodItemEatenStartingCalories);
			}

			if (giveFoodPoisoning) {
				if (MenuSettings.settings.delayedFoodPoisoning) {
					HungerRevamped.Instance.AddFoodPoisoningCall(foodItemEaten.m_LocalizedDisplayName.m_LocalizationID);
				} else {
					GameManager.GetFoodPoisoningComponent().FoodPoisoningStart(foodItemEaten.m_LocalizedDisplayName.m_LocalizationID, true, false);
				}
			}
		}

		private static bool CustomRollForFoodPoisoning(GearItem gearItem, float startingCalories) {
			FoodItem foodItem = gearItem.m_FoodItem;
			if (!foodItem || startingCalories < 5f) return false;
			if (foodItem.m_IsRawMeat) return gearItem.RollForFoodPoisoning(startingCalories); // Let the original method handle this. Raw meat is alright there

			float condition = gearItem.GetNormalizedCondition();
			if (condition >= 0.745f) return false; // Above (displayed) 75% condition -> no food poisoning

			float lowChance = foodItem.m_ChanceFoodPoisoning;
			float highChance = foodItem.m_ChanceFoodPoisoningLowCondition;

			if (lowChance > highChance) return gearItem.RollForFoodPoisoning(startingCalories); // No idea what's going on here, bail
			if (foodItem.m_CaloriesTotal <= 0f) return gearItem.RollForFoodPoisoning(startingCalories); // Similarly, avoid NaN or negative values

			float proportionEaten = (startingCalories - foodItem.m_CaloriesRemaining) / foodItem.m_CaloriesTotal;
			if (proportionEaten < 0.001f) return false; // No food poisoning for tiny amounts of food (even though that's somewhat unrealistic)

			float chanceFoodPoisioning = GetFoodPoisoningChance(lowChance, highChance, condition) / 100f;
			float scaledChance = 1f - Mathf.Pow(1f - chanceFoodPoisioning, proportionEaten);
			return Random.value < scaledChance;
		}

		private static float GetFoodPoisoningChance(float lowChance, float highChance, float condition) {
			float chanceAt45Percent = lowChance;
			float chanceAt25Percent = Mathf.Min(2 * lowChance, highChance);
			float chanceAt15Percent = highChance;

			if (condition > 0.45f) {
				return Mathf.Lerp(0f, chanceAt45Percent, Mathf.InverseLerp(0.745f, 0.45f, condition));
			} else if (condition > 0.25f) {
				return Mathf.Lerp(chanceAt45Percent, chanceAt25Percent, Mathf.InverseLerp(0.45f, 0.25f, condition));
			} else if (condition > 0.15f) {
				return Mathf.Lerp(chanceAt25Percent, chanceAt15Percent, Mathf.InverseLerp(0.25f, 0.15f, condition));
			} else {
				return chanceAt15Percent;
			}
		}
	}
}
