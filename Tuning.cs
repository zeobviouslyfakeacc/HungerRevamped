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

			float calorieRemoveSpeed = removeRatio01 / (1 - Mathf.Log10(storedCaloriesRatio));
			return Mathf.Max(removingCaloriesPerHour * calorieRemoveSpeed, -calorieBurnRate);
		}

		internal static float GetConditionChangePerHour(float hungerRatio, double storedCalories) {
			float starveRatio01 = Mathf.Clamp01((hungerLevelStarving - hungerRatio) / hungerLevelStarving);
			float noStoredCaloriesMultiplier = (storedCalories < 1)   ? 2.5f :
			                                   (storedCalories < 250) ? 2.0f :
			                                                            1.0f;
			return starvingConditionChangePerHour * starveRatio01 * noStoredCaloriesMultiplier;
		}

		internal static float GetStoredCaloriesWarmthBonus(float storedCaloriesRatio) {
			int rows = storedCaloriesWarmthBonus.GetLength(0);
			for (int i = 1; i < rows; ++i) {
				if (storedCaloriesRatio < storedCaloriesWarmthBonus[i, 0]) {
					float t = Mathf.InverseLerp(storedCaloriesWarmthBonus[i - 1, 0], storedCaloriesWarmthBonus[i, 0], storedCaloriesRatio);
					return Mathf.Lerp(storedCaloriesWarmthBonus[i - 1, 1], storedCaloriesWarmthBonus[i, 1], t);
				}
			}
			return storedCaloriesWarmthBonus[rows - 1, 1];
		}

		internal static float GetCalorieBurnRateMultiplier(float storedCalorieRatio) {
			float xSqr = storedCalorieRatio * storedCalorieRatio;
			return 1 + 0.4f * xSqr;
		}
	}
}
