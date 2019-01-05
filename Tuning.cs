using UnityEngine;

namespace HungerRevamped {
	internal static class Tuning {
		internal const float maximumHungerCalories = 2_500f;
		internal const double maximumStoredCalories = 20_000;
		internal const double startingStoredCalories = 12_000;
		internal const double defaultStoredCalories = 10_000;

		internal const float hungerLevelWellFed = 0.6f;
		internal const float hungerLevelMalnourished = 0.35f;
		internal const float hungerLevelStarving = 0.2f;

		internal const float storingCaloriesPerHour = 120f;
		internal const float removingCaloriesPerHour = -200f;
		internal const float starvingConditionChangePerHour = -5f;

		internal const float foodPoisoningDelayHoursMin = 4f;
		internal const float foodPoisoningDelayHoursMax = 16f;
		internal const float foodPoisoningPreventedByAntibioticsChance = 85f;

		internal const float wellFedCarryCapacityBuffMax = 7.5f;
		internal const float wellFedCarryBonusStart = 2.5f;
		internal const float wellFedCarryBonusEnd = 2f;

		internal const double startingWellFedHungerScore = -1;
		internal const double wellFedHungerScoreDecayPerHour = 0.98;

		internal static readonly float[,] wellFedHungerScoreChange = {
			{0f, -20f}, {hungerLevelStarving, -2.5f}, {hungerLevelMalnourished, 0f}, {hungerLevelWellFed, 0f}, {1f, 1.5f}
		};

		internal static readonly float[,] storedCaloriesWarmthBonus = {
			{0f, -3f}, {0.1f, -2f}, {0.25f, -1f}, {0.5f, 0f}, {0.8f, 1f}, {1f, 2f}
		};

		internal static float GetCaloriesStoredPerHour(float hungerRatio) {
			float storeRatio01 = Mathf.Clamp01((hungerRatio - hungerLevelWellFed) / (1 - hungerLevelWellFed));
			return storingCaloriesPerHour * Mathf.Sqrt(storeRatio01);
		}

		internal static float GetCaloriesRemovedPerHour(float hungerRatio, double storedCalories, float calorieBurnRate) {
			float removeRatio01 = Mathf.Clamp01((hungerLevelMalnourished - hungerRatio) / hungerLevelMalnourished);
			float storedCaloriesRatio = (float) (storedCalories / maximumStoredCalories);

			if (storedCaloriesRatio <= 0) // Empty calorie store, no more calories left to transfer
				return 0f;
			if (removeRatio01 > 0.999f) // Empty hunger bar, take all from stored calories
				return -calorieBurnRate;

			float maxCaloriesRemovedPerHour = GetCalorieBurnRateMultiplier(storedCaloriesRatio) * removingCaloriesPerHour;
			float calorieRemoveSpeed = removeRatio01 / (1 - Mathf.Log10(storedCaloriesRatio));
			return Mathf.Max(maxCaloriesRemovedPerHour * calorieRemoveSpeed, -calorieBurnRate);
		}

		internal static float GetConditionChangePerHour(float hungerRatio, double storedCalories) {
			float starveRatio01 = Mathf.Clamp01((hungerLevelStarving - hungerRatio) / hungerLevelStarving);
			float noStoredCaloriesMultiplier = (storedCalories < 1)   ? 2.5f :
			                                   (storedCalories < 250) ? 2.0f :
			                                                            1.0f;
			return starvingConditionChangePerHour * starveRatio01 * noStoredCaloriesMultiplier;
		}

		internal static float GetStoredCaloriesWarmthBonus(float storedCaloriesRatio) {
			return LerpFromArray(storedCaloriesWarmthBonus, storedCaloriesRatio);
		}

		internal static float GetCalorieBurnRateMultiplier(float storedCalorieRatio) {
			float xSqr = storedCalorieRatio * storedCalorieRatio;
			return 0.95f + 0.4f * xSqr;
		}

		internal static float GetWellFedHungerScoreChange(float hungerRatio) {
			return LerpFromArray(wellFedHungerScoreChange, hungerRatio);
		}

		internal static float GetCarryBonus(float hungerScore /* [-1, 1] */, float storedCaloriesScore /* [-1, 1] */) {
			float totalScore = storedCaloriesScore + hungerScore; /* [-2, 2] */
			float totalScore01 = Mathf.Clamp01(totalScore);
			return totalScore01 * wellFedCarryCapacityBuffMax;
		}

		private static float LerpFromArray(float[,] array, float x) {
			if (x < array[0, 0])
				return array[0, 1];

			int rows = array.GetLength(0);
			for (int i = 1; i < rows; ++i) {
				if (x < array[i, 0]) {
					float t = Mathf.InverseLerp(array[i - 1, 0], array[i, 0], x);
					return Mathf.Lerp(array[i - 1, 1], array[i, 1], t);
				}
			}
			return array[rows - 1, 1];
		}
	}
}
