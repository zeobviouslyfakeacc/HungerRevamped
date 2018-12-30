using Harmony;
using UnityEngine;

namespace HungerRevamped {
	internal static class DebugCommandPatches {

		[HarmonyPatch(typeof(ConsoleManager), "RegisterCommands")]
		private static class AddConsoleCommands {
			private static void Postfix() {
				uConsole.RegisterCommandReturn("get_stored_calories", new uConsole.DebugCommandReturn(GetStoredCalories));
				uConsole.RegisterCommand("set_stored_calories", new uConsole.DebugCommand(SetStoredCalories));
				uConsole.RegisterCommandReturn("get_calorie_burn", new uConsole.DebugCommandReturn(GetCalorieBurnRate));
				uConsole.RegisterCommandReturn("get_calorie_transfer", new uConsole.DebugCommandReturn(GetCalorieTransferRate));
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
		}
	}
}
