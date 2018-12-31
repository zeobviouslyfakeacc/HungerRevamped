using Harmony;
using UnityEngine;

namespace HungerRevamped {
	internal static class DebugCommandPatches {

		[HarmonyPatch(typeof(ConsoleManager), "RegisterCommands")]
		private static class AddConsoleCommands {
			private static void Postfix() {
				uConsole.RegisterCommandReturn("get_stored_calories", GetStoredCalories);
				uConsole.RegisterCommand("set_stored_calories", SetStoredCalories);
				uConsole.RegisterCommandReturn("get_calorie_burn", GetCalorieBurnRate);
				uConsole.RegisterCommandReturn("get_calorie_transfer", GetCalorieTransferRate);
				uConsole.RegisterCommandReturn("get_well_fed_hunger_score", GetWellFedHungerScore);
				uConsole.RegisterCommand("set_well_fed_hunger_score", SetWellFedHungerScore);
				uConsole.RegisterCommand("get_deferred_food_poisonings", GetDeferredFoodPoisonings);
			}

			private static object GetStoredCalories() {
				return HungerRevamped.Instance.storedCalories;
			}

			private static void SetStoredCalories() {
				if (uConsole.GetNumParameters() == 0)
					return;

				float calories = uConsole.GetFloat();
				calories = Mathf.Clamp(calories, 0f, (float) Tuning.maximumStoredCalories);
				HungerRevamped.Instance.storedCalories = calories;
			}

			private static object GetCalorieBurnRate() {
				return HungerRevamped.Instance.hunger.GetCurrentCalorieBurnPerHour();
			}

			private static object GetCalorieTransferRate() {
				return HungerRevamped.Instance.GetStoredCaloriesChangePerHour();
			}

			private static object GetWellFedHungerScore() {
				return HungerRevamped.Instance.wellFedHungerScore;
			}

			private static void SetWellFedHungerScore() {
				if (uConsole.GetNumParameters() == 0)
					return;

				float hungerScore = uConsole.GetFloat();
				hungerScore = Mathf.Clamp(hungerScore, -1, 1);
				HungerRevamped.Instance.wellFedHungerScore = hungerScore;
			}

			private static void GetDeferredFoodPoisonings() {
				float timeNow = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused();
				foreach (DeferredFoodPoisoning dfp in HungerRevamped.Instance.deferredFoodPoisonings) {
					float dHours = dfp.start - timeNow;
					uConsole.Log("Hour " + dfp.start + " caused by " + dfp.cause + " (in " + dHours + " hours)");
				}
			}
		}
	}
}
