using Harmony;
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

	internal static class Watch_OnEatingComplete {
		internal static bool isExecuting = false;

		internal static bool CustomRollForFoodPoisoning(GearItem gearItem, float startingCalories, float progress) {
			FoodItem foodItem = gearItem.m_FoodItem;
			
			if (!foodItem || startingCalories < 5f) return false;

			if (foodItem.m_IsRawMeat)  return gearItem.RollForFoodPoisoning(startingCalories); // Let the original method handle this. Raw meat is alright there

			float condition = gearItem.GetNormalizedCondition();
			if (condition >= 0.745f) return false;

			float lowChance = foodItem.m_ChanceFoodPoisoning;
			float highChance = foodItem.m_ChanceFoodPoisoningLowCondition;

			if (lowChance > highChance) return gearItem.RollForFoodPoisoning(startingCalories); // No idea what's going on here, bail

			float proportionEaten = (startingCalories - foodItem.m_CaloriesRemaining) / foodItem.m_CaloriesTotal;
			float chanceFoodPoisioning = GetFoodPoisoningChance(lowChance, highChance, condition) * proportionEaten;
			//MelonLoader.MelonLogger.Log("Food Poisoning Chance: '{0}'", chanceFoodPoisioning);
			float randomValue = UnityEngine.Random.value * 100;
			//MelonLoader.MelonLogger.Log("Random Value: '{0}'", randomValue);
			return randomValue < chanceFoodPoisioning;
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
			} else return chanceAt15Percent;
		}
	}

	[HarmonyPatch(typeof(PlayerManager),"OnEatingComplete")]
	internal static class FoodPoisoningChanceFix {
		private static void Prefix() {
			if (!MenuSettings.settings.realisticFoodPoisoningChance) return;
			Watch_OnEatingComplete.isExecuting = true;
        }
		private static void Postfix(PlayerManager __instance,float progress) {
			if (!MenuSettings.settings.realisticFoodPoisoningChance) return;
			Watch_OnEatingComplete.isExecuting = false;
			if (Watch_OnEatingComplete.CustomRollForFoodPoisoning(__instance.m_FoodItemEaten, __instance.m_FoodItemEatenStartingCalories, progress)) {
				GameManager.GetFoodPoisoningComponent().FoodPoisoningStart(__instance.m_FoodItemEaten.m_LocalizedDisplayName.m_LocalizationID, true, false);
			}
        }
	}
}
