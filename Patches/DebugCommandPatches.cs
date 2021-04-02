using Harmony;
using UnityEngine;
using System;

namespace HungerRevamped {
	internal static class DebugCommandPatches {

		[HarmonyPatch(typeof(ConsoleManager), "RegisterCommands")]
		private static class AddConsoleCommands {
			private static void Postfix() {
				uConsole.RegisterCommand("get_stored_calories", LogReturn(GetStoredCalories));
				uConsole.RegisterCommand("set_stored_calories", new Action(SetStoredCalories));
				uConsole.RegisterCommand("get_calorie_burn", LogReturn(GetCalorieBurnRate));
				uConsole.RegisterCommand("get_calorie_transfer", LogReturn(GetCalorieTransferRate));
				uConsole.RegisterCommand("get_well_fed_hunger_score", LogReturn(GetWellFedHungerScore));
				uConsole.RegisterCommand("set_well_fed_hunger_score", new Action(SetWellFedHungerScore));
				uConsole.RegisterCommand("get_deferred_food_poisonings", new Action(GetDeferredFoodPoisonings));
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

			private static Action LogReturn(Func<object> commandReturn) {
				return () => {
					object result = commandReturn();
					uConsole.Log(result == null ? "(null)" : result.ToString());
				};
			}
		}
	}
}
